# SX3.Tools.D3DRender64
Library for rendering on a Windows Form with SharpDX. Contains basic functions and a keyboard driven menu. Basically the same as the old [repo](https://github.com/SuRTaiNx3/SX3.Tools.D3DRender) but with x64 support.

## Usage
The code below shows a basic example. Please take a look at the SX3.Tools.D3DRender.Example project for a more detailed version.
```csharp
using using SX3.Tools.D3DRender64;

int width = 1920;
int height = 1080;
var ui = new UIRenderer(this.Handle);
ui.InitializeDevice(width, height);

new Thread(() => 
{
	while (true)
	{
		ui.StartFrame(width, height);

		ui.DrawBox(100, 100, 75, 150, 1, SharpDX.Mathematics.Color.Red);

		ui.EndFrame();
	}
}).Start();
```

## License
[MIT](https://choosealicense.com/licenses/mit/)
