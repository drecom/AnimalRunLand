using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsl.Entity
{
[System.Serializable]
public class GoogleGeoJson
{
    public G_result[] results;
    public string status;
}
[System.Serializable]
public class G_result
{
    public G_address_component[] address_components;
    public string formatted_address;
    public G_geometry geometry;
    public string place_id;
    public string[] types;
}
[System.Serializable]
public class G_address_component
{
    public string long_name;
    public string short_name;
    public string[] types;
}
[System.Serializable]
public class G_geometry
{
    public G_Rect bounds;
    public G_latlng location;
    public string location_type;
    public G_Rect viewport;
}
[System.Serializable]
public class G_Rect
{
    public G_latlng northeast;
    public G_latlng southwest;
}
[System.Serializable]
public class G_latlng
{
    public float lat;
    public float lng;
}
}
