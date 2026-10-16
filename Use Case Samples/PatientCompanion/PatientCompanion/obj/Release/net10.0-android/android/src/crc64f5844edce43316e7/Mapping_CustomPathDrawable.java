package crc64f5844edce43316e7;


public class Mapping_CustomPathDrawable
	extends android.graphics.drawable.Drawable
	implements
		mono.android.IGCUserPeer
{

	public Mapping_CustomPathDrawable ()
	{
		super ();
		if (getClass () == Mapping_CustomPathDrawable.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Rotator.Mapping+CustomPathDrawable, Syncfusion.Maui.Core", "", this, new java.lang.Object[] {  });
		}
	}

	public void draw (android.graphics.Canvas p0)
	{
		n_draw (p0);
	}

	private native void n_draw (android.graphics.Canvas p0);

	public void setAlpha (int p0)
	{
		n_setAlpha (p0);
	}

	private native void n_setAlpha (int p0);

	public void setColorFilter (android.graphics.ColorFilter p0)
	{
		n_setColorFilter (p0);
	}

	private native void n_setColorFilter (android.graphics.ColorFilter p0);

	public int getOpacity ()
	{
		return n_getOpacity ();
	}

	private native int n_getOpacity ();

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
