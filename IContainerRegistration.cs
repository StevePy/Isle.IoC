using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace Isle.IOC
{
	/// <summary>
	/// Registrations add the ability to adjust the Autofac container registration for 
	/// environment specific implementations such as registering services, repositories,
	/// or elements such as MVC controllers, or Web API controllers.
	/// </summary>
	public interface IContainerRegistration
	{
		/// <summary>
		/// Register the desired capabilities within the Autofac container.
		/// </summary>
		void Register(ContainerBuilder builder);
	}
}
