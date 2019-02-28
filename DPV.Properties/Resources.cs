using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace DPV.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					ResourceManager resourceManager = resourceMan = new ResourceManager("DPV.Properties.Resources", typeof(Resources).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static Bitmap ajax_loader_gray_48
		{
			get
			{
				object @object = ResourceManager.GetObject("ajax_loader_gray_48", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap arrow_down
		{
			get
			{
				object @object = ResourceManager.GetObject("arrow_down", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap arrow_up
		{
			get
			{
				object @object = ResourceManager.GetObject("arrow_up", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal static Bitmap icons8_graduation_cap_50
		{
			get
			{
				object @object = ResourceManager.GetObject("icons8-graduation-cap-50", resourceCulture);
				return (Bitmap)@object;
			}
		}

		internal Resources()
		{
		}
	}
}
