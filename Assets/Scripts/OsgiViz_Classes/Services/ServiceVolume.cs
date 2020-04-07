using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceVolume{

    public List<ServiceSlice> serviceSlices;

    public ServiceVolume()
    {
        serviceSlices = new List<ServiceSlice>();
    }
       
	
	
	

    /*
    private void updateSliceVisibility()
    {
        foreach (ServiceSlice slice in serviceSlices)
        {
            if (slice.height >= boundA.position.y && slice.height <= boundB.position.y)
                slice.showSlice();
            else
                slice.hideSlice();

        }
    }
    */

}
