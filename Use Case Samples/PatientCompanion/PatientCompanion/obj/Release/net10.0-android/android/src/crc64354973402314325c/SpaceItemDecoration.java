package crc64354973402314325c;


public class SpaceItemDecoration
	extends androidx.recyclerview.widget.RecyclerView.ItemDecoration
	implements
		mono.android.IGCUserPeer
{

	public SpaceItemDecoration ()
	{
		super ();
		if (getClass () == SpaceItemDecoration.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Carousel.SpaceItemDecoration, Syncfusion.Maui.Core", "", this, new java.lang.Object[] {  });
		}
	}

	public SpaceItemDecoration (int p0)
	{
		super ();
		if (getClass () == SpaceItemDecoration.class) {
			mono.android.TypeManager.Activate ("Syncfusion.Maui.Core.Carousel.SpaceItemDecoration, Syncfusion.Maui.Core", "System.Int32, System.Private.CoreLib", this, new java.lang.Object[] { p0 });
		}
	}

	public void getItemOffsets (android.graphics.Rect p0, android.view.View p1, androidx.recyclerview.widget.RecyclerView p2, androidx.recyclerview.widget.RecyclerView.State p3)
	{
		n_getItemOffsets (p0, p1, p2, p3);
	}

	private native void n_getItemOffsets (android.graphics.Rect p0, android.view.View p1, androidx.recyclerview.widget.RecyclerView p2, androidx.recyclerview.widget.RecyclerView.State p3);

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
