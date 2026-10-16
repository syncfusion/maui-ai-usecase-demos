package crc648a9457dd5f66374f;


public class LayoutViewGroupExtension
	extends crc6486af4e8104107bae.LayoutViewGroupExt
	implements
		mono.android.IGCUserPeer
{

	public LayoutViewGroupExtension (android.content.Context p0)
	{
		super (p0);
		if (getClass () == LayoutViewGroupExtension.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Hosting.LayoutViewGroupExtension, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
		}
	}

	public LayoutViewGroupExtension (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == LayoutViewGroupExtension.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Hosting.LayoutViewGroupExtension, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
		}
	}

	public LayoutViewGroupExtension (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == LayoutViewGroupExtension.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Hosting.LayoutViewGroupExtension, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, System.Private.CoreLib", this, new java.lang.Object[] { p0, p1, p2 });
		}
	}

	public LayoutViewGroupExtension (android.content.Context p0, android.util.AttributeSet p1, int p2, int p3)
	{
		super (p0, p1, p2, p3);
		if (getClass () == LayoutViewGroupExtension.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Hosting.LayoutViewGroupExtension, Syncfusion.Maui.Core", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, System.Private.CoreLib:System.Int32, System.Private.CoreLib", this, new java.lang.Object[] { p0, p1, p2, p3 });
		}
	}

	public void dispatchDraw (android.graphics.Canvas p0)
	{
		n_dispatchDraw (p0);
	}

	private native void n_dispatchDraw (android.graphics.Canvas p0);

	public void onMeasure (int p0, int p1)
	{
		n_onMeasure (p0, p1);
	}

	private native void n_onMeasure (int p0, int p1);

	public void onLayout (boolean p0, int p1, int p2, int p3, int p4)
	{
		n_onLayout (p0, p1, p2, p3, p4);
	}

	private native void n_onLayout (boolean p0, int p1, int p2, int p3, int p4);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
