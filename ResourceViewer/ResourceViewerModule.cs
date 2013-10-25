using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using JustDecompile.API.CompositeEvents;
using JustDecompile.API.Core;
using JustDecompile.API.Core.Services;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace ResourceViewerPlugin
{
    [ModuleExport(typeof(ResourceViewerModule))]
    // ReSharper disable InconsistentNaming
    public class ResourceViewerModule : IModule, IPartImportsSatisfiedNotification
    {
        [Import] private IAssemblyManagerService assemblyManagerService;
        private MenuItem assemblyNodeContextMenu;

        [Import] private IEventAggregator eventAggregator;

        [Import] private ITreeViewNavigatorService navigationService;
        [Import] private IRegionManager regionManager;
        private ITreeViewItem selectedItem;
        private ITypeDefinition selectedTypeDefinition;

        [Import] private ITabService tabService;
        private IEnumerable<ITreeViewItem> treeViewItems;

        public void Initialize()
        {
            regionManager.AddToRegion("AssemblyTreeViewContextMenuRegion", assemblyNodeContextMenu);
            //regionManager.AddToRegion("DefaultResourceTreeViewContextMenuRegion", assemblyNodeContextMenu);
            //regionManager.AddToRegion("ResourceTreeViewContextMenuRegion", assemblyNodeContextMenu);
        }

        public void OnImportsSatisfied()
        {
            assemblyNodeContextMenu = new MenuItem {Header = "Load Bitmaps", Command = new DelegateCommand(ViewBitmaps)};
            eventAggregator.GetEvent<SelectedTreeViewItemChangedEvent>().Subscribe(OnSelectedTreeViewItemChanged);
            eventAggregator.GetEvent<TreeViewItemCollectionChangedEvent>().Subscribe(OnTreeViewCollectionChanged);
        }

        private void ViewBitmaps()
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			IAssemblyDefinition assembly = GetAssemblyDefinition();

            var imb = new ImageBrowserView();
            regionManager.AddToRegion("PluginRegion", imb);
            imb.Closing += CloseView;
            imb.AssemblyName = Path.GetFileName(assembly.MainModule.FilePath);

            imb.Status = "Scanning for images...";
            imb.ProgressOverlayVisibility = Visibility.Visible;
            ResourceLoader.LoadBitmapsFromAssembly(assembly, i => imb.ProgressPercentage = i)
                .ContinueWith(r =>
                {
                    imb.Images.AddRange(r.Result);
                    imb.ProgressOverlayVisibility = Visibility.Collapsed;
                    imb.Status = string.Empty;
                }, scheduler);
        }

        private void CloseView(object sender, EventArgs eventArgs)
        {
            if (sender == null) return;

            if (regionManager.Regions["PluginRegion"].ActiveViews.Contains(sender))
                regionManager.Regions["PluginRegion"].Remove(sender);
        }

        private void OnTreeViewCollectionChanged(IEnumerable<ITreeViewItem> treeViewItems)
        {
            this.treeViewItems = treeViewItems;
        }

        private void OnSelectedTreeViewItemChanged(ITreeViewItem obj)
        {
            selectedItem = obj;
        }

        private IAssemblyDefinition GetAssemblyDefinition()
        {
            if (selectedItem == null)
            {
				return null;
            }

            switch (selectedItem.TreeNodeType)
            {
                case TreeNodeType.AssemblyDefinition:
                    return ((IAssemblyDefinitionTreeViewItem)selectedItem).AssemblyDefinition;

				/*
                case TreeNodeType.AssemblyModuleDefinition:
                    return ((IAssemblyModuleDefinitionTreeViewItem)selectedItem).ModuleDefinition.FilePath;
                case TreeNodeType.AssemblyResource:
                    return ((IResourceTreeViewItem)selectedItem).AssemblyFile;
                case TreeNodeType.NotSupported:
                    return ((INotSupportedTreeViewItem)selectedItem).FilePath;
				*/
                default:
                    return null;
            }
        }
    }
}