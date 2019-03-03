using System.Configuration;

namespace DpvClassLibrary.Configuration
{
	public class TimersConfigSection : ConfigurationSection
	{
		public const string Tag = "timersConfig";

		[ConfigurationProperty("timers")]
		public TimerInstantiationCollection Timers
		{
			get
			{
				return (TimerInstantiationCollection)base["timers"];
			}
		}
	}
}
