package crc6436e425876cb621d9;


public class FragmentLifecycleManager
	extends androidx.fragment.app.FragmentManager.FragmentLifecycleCallbacks
	implements
		mono.android.IGCUserPeer
{

	public FragmentLifecycleManager ()
	{
		super ();
		if (getClass () == FragmentLifecycleManager.class) {
			mono.android.TypeManager.Activate ("CommunityToolkit.Maui.Core.Services.FragmentLifecycleManager, CommunityToolkit.Maui.Core", "", this, new java.lang.Object[] {  });
		}
	}

	public void onFragmentAttached (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.content.Context p2)
	{
		n_onFragmentAttached (p0, p1, p2);
	}

	private native void n_onFragmentAttached (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.content.Context p2);

	public void onFragmentCreated (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.os.Bundle p2)
	{
		n_onFragmentCreated (p0, p1, p2);
	}

	private native void n_onFragmentCreated (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.os.Bundle p2);

	public void onFragmentDestroyed (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1)
	{
		n_onFragmentDestroyed (p0, p1);
	}

	private native void n_onFragmentDestroyed (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1);

	public void onFragmentDetached (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1)
	{
		n_onFragmentDetached (p0, p1);
	}

	private native void n_onFragmentDetached (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1);

	public void onFragmentPaused (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1)
	{
		n_onFragmentPaused (p0, p1);
	}

	private native void n_onFragmentPaused (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1);

	public void onFragmentPreAttached (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.content.Context p2)
	{
		n_onFragmentPreAttached (p0, p1, p2);
	}

	private native void n_onFragmentPreAttached (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.content.Context p2);

	public void onFragmentPreCreated (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.os.Bundle p2)
	{
		n_onFragmentPreCreated (p0, p1, p2);
	}

	private native void n_onFragmentPreCreated (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.os.Bundle p2);

	public void onFragmentResumed (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1)
	{
		n_onFragmentResumed (p0, p1);
	}

	private native void n_onFragmentResumed (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1);

	public void onFragmentSaveInstanceState (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.os.Bundle p2)
	{
		n_onFragmentSaveInstanceState (p0, p1, p2);
	}

	private native void n_onFragmentSaveInstanceState (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.os.Bundle p2);

	public void onFragmentStarted (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1)
	{
		n_onFragmentStarted (p0, p1);
	}

	private native void n_onFragmentStarted (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1);

	public void onFragmentStopped (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1)
	{
		n_onFragmentStopped (p0, p1);
	}

	private native void n_onFragmentStopped (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1);

	public void onFragmentViewCreated (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.view.View p2, android.os.Bundle p3)
	{
		n_onFragmentViewCreated (p0, p1, p2, p3);
	}

	private native void n_onFragmentViewCreated (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1, android.view.View p2, android.os.Bundle p3);

	public void onFragmentViewDestroyed (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1)
	{
		n_onFragmentViewDestroyed (p0, p1);
	}

	private native void n_onFragmentViewDestroyed (androidx.fragment.app.FragmentManager p0, androidx.fragment.app.Fragment p1);

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
