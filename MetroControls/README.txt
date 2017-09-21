This is a WPF library with bits of C# that contains lots of styles and templates to build desktop Metro apps (nothing to do with Win8's Metro-style apps).

To use it, add a metro xmlns to your XAML files, and change all of your Windows to be metro:MetroWindows.
In your app.xaml file, add the following lines:

    <ResourceDictionary Source="/MetroControls;component/Resources/MetroTemplates.xaml" />
    <ResourceDictionary Source="/MetroControls;component/Resources/MetroStyles.xaml" />
    <ResourceDictionary Source="/MetroControls;component/Resources/MetroWindowDefaultStyle.xaml" />

You're done. Build your app.

The ColorManager app is used to switch and add colors.
By default, two main colors are available: Black and White.
To add your own color, use the AddMainColorDictionary method with a new color. Look at the existing dictionaries in the Colors folder to build your own.
To add more resources to an existing color, use the AddMainColorDictionary method with an existing color.

You can set any color as the accent color.