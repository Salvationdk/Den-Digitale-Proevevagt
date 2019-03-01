using System.Configuration;

namespace DpvClassLibrary.Configuration
{
	public class TimerInstantiation : ConfigurationElement
	{
		public const string Tag = "timer";

		[ConfigurationProperty("secondsBetweenWork", DefaultValue = "60", IsRequired = true)]
		public int SecondsBetweenWork
		{
			get
			{
				return (int)base["secondsBetweenWork"];
			}
		}

		[ConfigurationProperty("workerToInstantiate", DefaultValue = "", IsRequired = true)]
		public string WorkerToInstantiate
		{
			get
			{
				return (string)base["workerToInstantiate"];
			}
		}
	}
}
