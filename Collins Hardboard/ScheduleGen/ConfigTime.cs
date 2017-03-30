using System;
using Configuration_windows;

class ConfigTime
{
    public ConfigurationGroup Group { get; set; }
    public DateTime Time { get; set; }

    public ConfigTime(ConfigurationGroup group, DateTime time)
    {
        Group = group;
        Time = time;
    }
}