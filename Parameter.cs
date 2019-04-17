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

namespace Isle.IOC
{
	/// <summary>
	/// A simple wrapper class for parameters to be sent through the IOC for initializiation.
	/// </summary>
	public class Parameter
	{
		public string Name
		{
			get;
			set;
		}

		public object ParameterValue
		{
			get;
			set;
		}
	}
}
