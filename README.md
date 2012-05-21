# UnityTest

This package provides an xUnit style testing infrastructure for Unity projects,

Key features:

* In-editor and in-player test runners.
* Controller to run test suites from multiple scenes in a single run.
* Run tests at a variety of explicitly controlled framerates to find
  high/low-FPS bugs.


## Requirements

* Unity 3.x.


## Install

Grab the latest .unityPackage and install it into your project:

    http://github.com/MrJoy/UnityTest/downloads

OR, if you are using Git for your Unity project, you can add it as a sub-module:

    git submodule add git://github.com/MrJoy/UnityTest.git Assets/UnityTest
    git submodule init
    git submodule update


## Source

UnityTest' Git repo is available on GitHub, which can be browsed at:

    http://github.com/MrJoy/UnityTest

and cloned with:

    git clone git://github.com/MrJoy/UnityTest.git


An example Unity project (which refers to this project as a sub-module) is
available here:

    http://github.com/MrJoy/UnityTest.Examples

and can be cloned with:

    git clone git://github.com/MrJoy/UnityTest.Examples.git


## Usage

See example project.


## License

Copyright (c) 2009-2012 MrJoy, Inc.

Licensed under the [BSD 2-Clause license](http://opensource.org/licenses/bsd-license.php).


## Contributing

If you'd like to contribute to UnityTest, I ask that you fork
MrJoy/UnityTest on GitHub, and push up a topic branch for each feature
you add or bug you fix.  Then create a pull request and explain what your code
does. This allows us to discuss and merge each change separately.
