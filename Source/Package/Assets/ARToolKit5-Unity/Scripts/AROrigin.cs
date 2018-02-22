/*
 *  AROrigin.cs
 *  ARToolKit for Unity
 *
 *  This file is part of ARToolKit for Unity.
 *
 *  ARToolKit for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ARToolKit for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with ARToolKit for Unity.  If not, see <http://www.gnu.org/licenses/>.
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
	public List<String> findMarkerTags = new List<string>();

	private ARMarker baseMarker = null;
	private List<ARMarker> markersEligibleForBaseMarker = new List<ARMarker>();

	[SerializeField]
	private FindMode _findMarkerMode = FindMode.AutoAll;

	public FindMode findMarkerMode
	{
		get
		{
			return _findMarkerMode;
		}
		
		set
		{
			if (_findMarkerMode != value) {
				_findMarkerMode = value;
				FindMarkers();
			}
		}
	}

	public void AddMarker(ARMarker marker, bool atHeadOfList = false)
	{
		if (!atHeadOfList) {
			markersEligibleForBaseMarker.Add(marker);
		} else {
			markersEligibleForBaseMarker.Insert(0, marker);
		}
	}

	public bool RemoveMarker(ARMarker marker)
	{
		if (baseMarker == marker) baseMarker = null;
		return markersEligibleForBaseMarker.Remove(marker);
	}
	
	public void RemoveAllMarkers()
	{
		baseMarker = null;
		markersEligibleForBaseMarker.Clear();
	}

	public void FindMarkers()
	{
		RemoveAllMarkers();
		if (findMarkerMode != FindMode.Manual) {
			ARMarker[] ms = FindObjectsOfType<ARMarker>(); // Does not find inactive objects.
			foreach (ARMarker m in ms) {
				if (findMarkerMode == FindMode.AutoAll || (findMarkerMode == FindMode.AutoByTags && findMarkerTags.Contains(m.Tag))) {
					markersEligibleForBaseMarker.Add(m);
				}
			}
			ARController.Log(LogTag + "Found " + markersEligibleForBaseMarker.Count + " markers eligible to become base marker.");
		}
	}

	void Start()
	{
		FindMarkers();
	}

	// Get the marker, if any, currently acting as the base.
	public ARMarker GetBaseMarker()
	{
		if (baseMarker != null) {
			if (baseMarker.Visible) return baseMarker;
			else baseMarker = null;
		}
		
		foreach (ARMarker m in markersEligibleForBaseMarker) {
			if (m.Visible) {
				baseMarker = m;
				//ARController.Log("Marker " + m.UID + " became base marker.");
				break;
			}
		}
		
		return baseMarker;
	}
	
	void OnApplicationQuit()
	{
		RemoveAllMarkers();
	}
}

