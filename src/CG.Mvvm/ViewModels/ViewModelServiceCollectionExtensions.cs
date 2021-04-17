using CG;
using CG.Mvvm.ViewModels;
using CG.Validations;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// This class contains extension methods related to the <see cref="IServiceCollection"/>
    /// type, for registering types related to view models.
    /// </summary>
    public static partial class ViewModelServiceCollectionExtensions
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method adds any custom view-model types to the specified service 
        /// collection object as scoped services.
        /// </summary>
        /// <param name="serviceCollection">The service collection to use for
        /// the operation.</param>
        /// <param name="assemblyWhiteList">An optional white list, for filtering
        /// the assemblies used in the operation.</param>
        /// <param name="assemblyBlackList">An optional black list, for filtering
        /// the assemblies used in the operation.</param>
        /// <param name="serviceLifetime">The service lifetime to use for the operation.</param>
        /// <returns>The value of the <paramref name="serviceCollection"/>
        /// parameter, for chaining calls together.</returns>
        /// <remarks>
        /// This idea, with this method, is to dynamically locate and register
        /// any concrete view-model types. This way, we avoid having view-model 
        /// registration process turn into a maintenance issue.
        /// </remarks>
        public static IServiceCollection AddViewModels(
            this IServiceCollection serviceCollection,
            string assemblyWhiteList = "",
            string assemblyBlackList = "Microsoft.*,System.*,netstandard",
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(serviceCollection, nameof(serviceCollection));

            // Find all concrete types that derive from ViewModeBase.
            var impTypes = typeof(ViewModelBase).DerivedTypes(
                assemblyWhiteList,
                assemblyBlackList
                );

            // Loop through any types we found.
            foreach (var impType in impTypes)
            {
                // Look for a corresponding service interface type, which, if
                //   there is one, should be implemented by the type and derive 
                //   from the IViewModel interface.

                var serviceType = impType.FindInterfaces((x, y) =>
                {
                    // Look through all the types.
                    foreach (var z in y as Type[])
                    {
                        // Ignore same type.
                        if (z == x)
                        {
                            continue;
                        }

                        // Watch for assignable types (think inheritence).
                        if (z.IsAssignableFrom(x))
                        {
                            return true;
                        }
                    }

                    // Nothing found.
                    return false;
                },
                new[] { typeof(IViewModel) }
                ).FirstOrDefault();

                // Register the view-model type, with or without an associated 
                //   service type, depending on the previous search operation.

                // Did we find a service type?
                if (null != serviceType)
                {
                    // Register the view-model type with the service type.
                    serviceCollection.Add(
                        serviceType, 
                        impType, 
                        serviceLifetime
                        );
                }
                else
                {
                    // Otherwise, as a fall-back, register the view-model alone, 
                    //   without a corresponding service type.
                    serviceCollection.Add(
                        impType,
                        serviceLifetime
                        );
                }
            }

            // Return the service collection.
            return serviceCollection;
        }

        #endregion
    }
}
