using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Isle.IOC
{
    public interface IContainerRegistry
    {
		/// <summary>
		/// Adds a supported registration implementation for the container to configure the builder with.
		/// </summary>
		void AddRegistration(IContainerRegistration registration);

        /// <summary>
        /// Register the given type as a provided interface.
        /// </summary>
        void Register<T, I>(params IOC.Parameter[] parameters)
            where T : class
            where I : class;

        /// <summary>
        /// Register the given type as either of the provided interfaces.
        /// </summary>
        void Register<T, I1, I2>(params IOC.Parameter[] parameters)
            where T : class
            where I1 : class
            where I2 : class;

        /// <summary>
        /// Register the given instance of an object as a Singleton under the provided interface.
        /// </summary>
        void RegisterSingleton<I>(I instance) 
            where I : class;

        /// <summary>
        /// Register the given instance of an object as a Singleton under the provided interface.
        /// </summary>
        void RegisterSingleton<T, I>(T instance)
            where T : class
            where I : class;

        /// <summary>
        /// Register the given instance of an object as a Singleton under the provided interface.
        /// </summary>
        void RegisterSingleton<T, I>(params IOC.Parameter[] parameters)
            where T : class
            where I : class;

        /// <summary>
        /// Register the given instance of an object as a Singleton under the provided interfaces.
        /// </summary>
        void RegisterSingleton<T, I1, I2>(params IOC.Parameter[] parameters)
            where T : class
            where I1 : class
            where I2 : class;

        /// <summary>
        /// Register all types in the given set of assemblies excluding the provided list of types
        /// and filtering the remaining types by a suffix filter.
        /// </summary>
        /// <remarks>
        /// It is recommended to use a convention such as using suffixes like "Service" or "Repository" 
        /// for classes that are registered en-mass.
        /// </remarks>
        void RegisterInAssemblies(IEnumerable<Assembly> assemblies, IEnumerable<Type> excludeTypes, IEnumerable<string> suffixFilters);
        void RegisterInAssembliesPerWebRequest(IEnumerable<Assembly> assemblies, IEnumerable<Type> excludeTypes, IEnumerable<string> suffixFilters);

        /// <summary>
        /// Attempts to resolve the provided interface type from the IOC container.
        /// If the type cannot be resolved and exception is raised.
        /// </summary>
        I Resolve<I>()
            where I : class;

        /// <summary>
        /// Attempts to resolve the provided interface type from the IOC container.
        /// If the type cannot be resolved then the default value is returned.
        /// </summary>
        I TryResolve<I>(I defaultValue = default(I))
            where I : class;

    }
}
