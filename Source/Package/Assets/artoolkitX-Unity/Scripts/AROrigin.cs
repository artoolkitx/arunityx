/*
 *  AROrigin.cs
 *  artoolkitX for Unity
 *
 *  This file is part of artoolkitX for Unity.
 *
 *  artoolkitX for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  artoolkitX for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with artoolkitX for Unity.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  As a special exception, the copyright holders of this library give you
 *  permission to link this library with independent modules to produce an
 *  executable, regardless of the license terms of these independent modules, and to
 *  copy and distribute the resulting executable under terms of your choice,
 *  provided that you also meet, for each linked independent module, the terms and
 *  conditions of the license of that module. An independent module is a module
 *  which is neither derived from nor based on this library. If you modify this
 *  library, you may extend this exception to your version of the library, but you
 *  are not obligated to do so. If you do not wish to do so, delete this exception
 *  statement from your version.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Philip Lamb
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Transform))]
[ExecuteInEditMode]
public class AROrigin : MonoBehaviour
{
	private const string LogTag = "AROrigin: ";

	public enum FindMode {
		AutoAll,
		AutoByTags,
		Manual
	}
    public List<String> findTrackableTags = new List<string>();

	private ARTrackable baseTrackable = null;
    private List<ARTrackable> trackablesEligibleForBaseTrackable = new List<ARTrackable>();

	[SerializeField]
    private FindMode _findTrackableMode = FindMode.AutoAll;

	public FindMode findTrackableMode
	{
		get
		{
			return _findTrackableMode;
		}
		
		set
		{
			if (_findTrackableMode != value) {
				_findTrackableMode = value;
				FindTrackables();
			}
		}
	}

    public void AddTrackable(ARTrackable trackable, bool atHeadOfList = false)
	{
		if (!atHeadOfList) {
			trackablesEligibleForBaseTrackable.Add(trackable);
		} else {
            trackablesEligibleForBaseTrackable.Insert(0, trackable);
		}
	}

    public bool RemoveTrackable(ARTrackable trackable)
	{
		if (baseTrackable == trackable) baseTrackable = null;
        return trackablesEligibleForBaseTrackable.Remove(trackable);
	}
	
    public void RemoveAllTrackables()
	{
		baseTrackable = null;
        trackablesEligibleForBaseTrackable.Clear();
	}

    public void FindTrackables()
	{
		RemoveAllTrackables();
		if (findTrackableMode != FindMode.Manual) {
			ARTrackable[] ms = FindObjectsOfType<ARTrackable>(); // Does not find inactive objects.
			foreach (ARTrackable m in ms) {
				if (findTrackableMode == FindMode.AutoAll || (findTrackableMode == FindMode.AutoByTags && findTrackableTags.Contains(m.Tag))) {
                    trackablesEligibleForBaseTrackable.Add(m);
				}
			}
            ARController.Log(LogTag + "Found " + trackablesEligibleForBaseTrackable.Count + " trackables eligible to become base trackable.");
		}
	}

	void Start()
	{
		FindTrackables();
	}

    // Get the trackable, if any, currently acting as the base.
	public ARTrackable GetBaseTrackable()
	{
        if (baseTrackable != null) {
			if (baseTrackable.Visible) return baseTrackable;
			else baseTrackable = null;
		}
        foreach (ARTrackable m in trackablesEligibleForBaseTrackable) {
			if (m.Visible) {
				baseTrackable = m;
				ARController.Log("Trackable " + m.UID + " became base trackable.");
				break;
			}
		}
		
		return baseTrackable;
	}
	
	void OnApplicationQuit()
	{
		RemoveAllTrackables();
	}
}

