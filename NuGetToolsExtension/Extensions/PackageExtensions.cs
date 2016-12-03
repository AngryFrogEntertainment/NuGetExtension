using System;
using EnvDTE80;
using EnvDTE;

namespace AngryFrog.NuGetToolsExtension.Extensions
{
    internal static class PackageExtensions
    {
        public static T GetSelectedFileFromSolutionExplorer<T>(this IServiceProvider package)
        {
            T item = default(T);

            var dte = (DTE2)package.GetService(typeof(DTE));

            if (dte != null)
            {
                var selectedItems = dte.ToolWindows.SolutionExplorer.SelectedItems as Array;

                if (selectedItems != null && selectedItems.Length == 1)
                {
                    var selectedItem = selectedItems.GetValue(0) as UIHierarchyItem;

                    if (selectedItem != null)
                    {
                        if (selectedItem.Object is T)
                        {
                            item = (T)selectedItem.Object;
                        }
                    }
                }
            }

            return item;
        }
    }
}
