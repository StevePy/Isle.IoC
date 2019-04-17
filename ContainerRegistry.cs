/* Copyright 2011 Steve Pynylo
 *  All Rights Reserved.
 * 
 * All information contained herein is, and remains the property of Steve Pynylo. 
 * The intellectual and technical concepts contained herein are proprietary to 
 * Steve Pynylo under the trading name Isle. and may be covered by 
 * Australian and Foreign Patents, patents in process, and are protected by 
 * trade secret or copyright law.
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from Steve Pynylo.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace Isle.IOC
{
	/// <summary>
	/// A registry class to wrap the Autofac IOC Container. This provides basic
	/// AutoFac support without requiring Autofac references within the application
	/// and simplifies registering and retrieving contained objects.
	/// </summary>
	/// <remarks>
	/// This class is more accurately a service locator implementation for the Autofac IOC 
	/// container, however it is *not* intended to be used loosely as a service locator.
	/// To use this registry, structure your application to Register all classes and Singletons
	/// at the start, then use Resolve to pull those registered instances at any time afterwards.
	/// 
	/// The pattern this registry is intended for is lazy property injection. The registry should be
	/// the sole dependency injected via constructor with all other dependencies set up using
	/// the lazy property signature below:
	/// 
	/// private readonly IContainerRegistry _containerRegistry = null; // Initialized in constructor.
	/// private TDependency _dependency = null;
	/// public TDependency Dependency
	/// {
	///     get { return _dependency ?? ( _dependency = _containerRegistry.Resolve<TDependency>()); }
	///     set { _dependency = value; }
	/// }
	/// 
	/// This class serves as a Singleton within the application. It should register itself within itself under
	/// the contract interface IContainerRegistry to inject itself into constructed classes. Worst case in
	/// legacy applications where adding constructor parameters or non-IoC initialized classes you
	/// can opt to use the Singleton Instance directly, though aim to only ever use it in Lazy Properties.
	/// 
	/// Currently this class is not explicitly Thread-Safe.
	/// </remarks>
	public class ContainerRegistry : IContainerRegistry
	{
		protected volatile ContainerBuilder _builder = new ContainerBuilder();

		private Dictionary<string, IContainerRegistration> _registrations = new Dictionary<string, IContainerRegistration>();

		void IContainerRegistry.AddRegistration(IContainerRegistration registration)
		{
			if (registration == null)
				throw new ArgumentNullException("registration");

			if (_container != null)
				throw new InvalidOperationException("Registrations can only be added prior to the container being built and accessed.");

			string registrationName = registration.GetType().Name;

			if (_registrations.ContainsKey(registrationName))
				return;

			_registrations.Add(registrationName, registration);
		}

		/// <summary>
		/// Singleton instance for this class.
		/// </summary>
		public readonly static IContainerRegistry Instance = new ContainerRegistry();

		protected IContainerRegistry This
		{
			get { return this; }
		}

		private volatile IContainer _container = null;
		/// <summary>
		/// Returns the container, building it if necessary.
		/// </summary>
		protected IContainer Container
		{
			get
			{
                lock (_builder)
                {
					if (_container == null)
					{
						processRegistrations();
						_container = _builder.Build();
					}
                    return _container;
                }
			}
		}

		private void processRegistrations()
		{
			foreach (var registration in _registrations.Values)
				registration.Register(_builder);
		}

		/// <summary>
		/// This class is a Singleton and cannot be instantiated.
		/// </summary>
		protected ContainerRegistry()
		{
			This.RegisterSingleton<IContainerRegistry>(This);
		}

        void IContainerRegistry.Register<T, I>(params IOC.Parameter[] parameters)
		{
			if (parameters.Length > 0)
			{
				var namedParameters = convertParameters(parameters);
				_builder.RegisterType<T>().As<I>().WithParameters(namedParameters);
			}
			else
				_builder.RegisterType<T>().As<I>();
		}

        void IContainerRegistry.Register<T, I1, I2>(params IOC.Parameter[] parameters)
		{
			if (parameters.Length > 0)
			{
				var namedParameters = convertParameters(parameters);
				_builder.RegisterType<T>().As<I1>().As<I2>().WithParameters(namedParameters);
			}
			else
				_builder.RegisterType<T>().As<I1>().As<I2>();
		}

        void IContainerRegistry.RegisterSingleton<T, I>(T instance)
		{
			_builder.Register(c=> instance).As<I>().SingleInstance();
		}

        void IContainerRegistry.RegisterSingleton<T, I>(params IOC.Parameter[] parameters)
		{
			if (parameters.Length > 0)
			{
				var namedParameters = convertParameters( parameters );
				_builder.RegisterType<T>().As<I>().SingleInstance().WithParameters( namedParameters );
			}
			else
				_builder.RegisterType<T>().As<I>().SingleInstance();
		}

        void IContainerRegistry.RegisterSingleton<I>(I instance)
        {
            _builder.Register(c => instance).As<I>().SingleInstance();
        }

        void IContainerRegistry.RegisterSingleton<T, I1, I2>(params IOC.Parameter[] parameters)
		{
			if (parameters.Length > 0)
			{
				var namedParameters = convertParameters( parameters );
				_builder.RegisterType<T>().As<I1>().As<I2>().SingleInstance().WithParameters(namedParameters);
			}
			else
				_builder.RegisterType<T>().As<I1>().As<I2>().SingleInstance();
		}

        void IContainerRegistry.RegisterInAssemblies(IEnumerable<Assembly> assemblies, IEnumerable<Type> excludeTypes, IEnumerable<string> suffixFilters)
        {
            if (!assemblies.Any())
                return;

            var registration = _builder.RegisterAssemblyTypes(assemblies.ToArray())
                .Where(t => false == excludeTypes.Contains(t));

            if (true == suffixFilters.Any())
                registration = registration.Where(t=> suffixFilters.Any(f=> t.Name.EndsWith(f)));

             registration.AsImplementedInterfaces();
        }

        void IContainerRegistry.RegisterInAssembliesPerWebRequest(IEnumerable<Assembly> assemblies, IEnumerable<Type> excludeTypes, IEnumerable<string> suffixFilters)
        {
            if (!assemblies.Any())
                return;

            var registration = _builder.RegisterAssemblyTypes(assemblies.ToArray())
                .Where(t => false == excludeTypes.Contains(t));

            if (true == suffixFilters.Any())
                registration = registration.Where(t => suffixFilters.Any(f => t.Name.EndsWith(f)));

            registration.AsImplementedInterfaces().InstancePerRequest();
        }


        I IContainerRegistry.Resolve<I>()
		{
			try
			{
				return Container.Resolve<I>();
			}
			catch (Autofac.Core.Registration.ComponentNotRegisteredException ex)
			{
                throw new InvalidOperationException(string.Format("A suitable implementation for type: {0} was not registered.", typeof(I).Name), ex);
			}
		}

        I IContainerRegistry.TryResolve<I>(I defaultValue)
        {
            try
            {
                return Container.Resolve<I>();
            }
            catch (Autofac.Core.Registration.ComponentNotRegisteredException)
            {	// If no component is registered, return default.
                return defaultValue;
            }
        }

        /// <summary>
        /// Converts the set of parameters into named parameters for Autofac to parse.
        /// </summary>
        private IEnumerable<NamedParameter> convertParameters(IEnumerable<IOC.Parameter> parameters)
        {
            var namedParameters = new List<NamedParameter>();
            foreach (var parameter in parameters)
            {
                namedParameters.Add(new NamedParameter(parameter.Name, parameter.ParameterValue));
            }
            return namedParameters;
        }

	}
}
