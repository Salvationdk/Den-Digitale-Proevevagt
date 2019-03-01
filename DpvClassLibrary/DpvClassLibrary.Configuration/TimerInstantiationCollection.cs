using System.Configuration;

namespace DpvClassLibrary.Configuration
{
	[ConfigurationCollection(typeof(TimerInstantiation), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "timer")]
	public class TimerInstantiationCollection : ConfigurationElementCollection
	{
		public const string Tag = "timers";

		protected override string ElementName => "timer";

		public TimerInstantiation this[int index]
		{
			get
			{
				return (TimerInstantiation)BaseGet(index);
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				base.BaseAdd(index, value);
			}
		}

		public new TimerInstantiation this[string name]
		{
			get
			{
				return (TimerInstantiation)BaseGet(name);
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new TimerInstantiation();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return element as TimerInstantiation;
		}
	}
}
