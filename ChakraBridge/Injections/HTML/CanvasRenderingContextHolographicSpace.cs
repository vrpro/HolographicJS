using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.Graphics.Holographic;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Runtime.InteropServices;
using OpenGL;
using Windows.Foundation.Collections;
using Windows.Perception.Spatial;

namespace ChakraBridge
{
    class CanvasRenderingContextHolographicSpace : ICanvasRenderingContextHolographicSpace
    {
        const int EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER = 0x320B;
        const int EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE = 0x320F;
        const int EGL_ANGLE_SURFACE_RENDER_TO_BACK_BUFFER = 0x320C;
        const int EGL_PLATFORM_ANGLE_ANGLE = 0x3202;
        const int EGL_PLATFORM_ANGLE_TYPE_ANGLE = 0x3203;
        const int EGL_PLATFORM_ANGLE_MAX_VERSION_MAJOR_ANGLE = 0x3204;
        const int EGL_PLATFORM_ANGLE_MAX_VERSION_MINOR_ANGLE = 0x3205;
        const int EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE = 0x3208;
        const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_ANGLE = 0x3209;
        const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_WARP_ANGLE = 0x320B;

        private Window window;
        private IDeviceContext deviceContext;
        private IntPtr context;
        private IntPtr surface;
        private IntPtr display;
        private GCHandle holographicSpaceHandle;
        private GCHandle stationaryReferenceFrameHandle;

        public IHTMLCanvasElement canvas { get; }

        internal CanvasRenderingContextHolographicSpace(Window window, IHTMLCanvasElement canvas)
        {
            this.window = window;
            this.canvas = canvas;

            context = CreateEglContext(window.holographicSpace, window.stationaryReferenceFrame);
            deviceContext.MakeCurrent(context);
        }

        //~CanvasRenderingContextHolographicSpace()
        //{
        //    deviceContext.MakeCurrent(IntPtr.Zero);
        //    deviceContext.DeleteContext(context);
        //}

        private IntPtr CreateEglContext(HolographicSpace holographicSpace, SpatialStationaryFrameOfReference stationaryReferenceFrame)
        {
            IntPtr context;
            int[] configAttribs = new int[]
            {
                Egl.RED_SIZE, 8,
                Egl.GREEN_SIZE, 8,
                Egl.BLUE_SIZE, 8,
                Egl.ALPHA_SIZE, 8,
                Egl.DEPTH_SIZE, 8,
                Egl.STENCIL_SIZE, 8,
                Egl.NONE
            };

            int[] contextAttribs = new int[]
            {
                Egl.CONTEXT_CLIENT_VERSION, 2,
                Egl.NONE
            };

            int[] surfaceAttribs = new int[] {
                EGL_ANGLE_SURFACE_RENDER_TO_BACK_BUFFER, Egl.TRUE,
                Egl.NONE
            };

            int[] defaultDisplayAttribs = new int[]
            {
                EGL_PLATFORM_ANGLE_TYPE_ANGLE, EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
                EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER, Egl.TRUE,
                EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.TRUE,
                Egl.NONE
            };

            int[] fl9_3DisplayAttributes = new int[]
            {
                // These can be used to request ANGLE's D3D11 renderer, with D3D11 Feature Level 9_3.
                // These attributes are used if the call to eglInitialize fails with the default display attributes.
                EGL_PLATFORM_ANGLE_TYPE_ANGLE, EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
                EGL_PLATFORM_ANGLE_MAX_VERSION_MAJOR_ANGLE, 9,
                EGL_PLATFORM_ANGLE_MAX_VERSION_MINOR_ANGLE, 3,
                EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER, Egl.TRUE,
                EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.TRUE,
                Egl.NONE
            };

            int[] warpDisplayAttributes = new int[]
            {
                // These attributes can be used to request D3D11 WARP.
                // They are used if eglInitialize fails with both the default display attributes and the 9_3 display attributes.
                EGL_PLATFORM_ANGLE_TYPE_ANGLE, EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE,
                EGL_PLATFORM_ANGLE_DEVICE_TYPE_ANGLE, EGL_PLATFORM_ANGLE_DEVICE_TYPE_WARP_ANGLE,
                EGL_ANGLE_DISPLAY_ALLOW_RENDER_TO_BACK_BUFFER, Egl.TRUE,
                EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE, Egl.TRUE,
                Egl.NONE
            };

            int[] configCount = new int[1];
            IntPtr[] configs = new IntPtr[8];

            if (Egl.BindAPI(Egl.OPENGL_ES_API) == false)
                throw new InvalidOperationException("No OpenGL ES API");

            if ((display = Egl.GetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE, IntPtr.Zero, defaultDisplayAttribs)) == IntPtr.Zero)
                throw new InvalidOperationException("Unable to get EGL display");

            if (!Egl.Initialize(display, null, null))
            {
                if ((display = Egl.GetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE, IntPtr.Zero, fl9_3DisplayAttributes)) == IntPtr.Zero)
                    throw new InvalidOperationException("Unable to get EGL display");

                if (!Egl.Initialize(display, null, null))
                {
                    if ((display = Egl.GetPlatformDisplayEXT(EGL_PLATFORM_ANGLE_ANGLE, IntPtr.Zero, warpDisplayAttributes)) == IntPtr.Zero)
                        throw new InvalidOperationException("Unable to get EGL display");

                    if (!Egl.Initialize(display, null, null))
                        throw new InvalidOperationException("Unable to initialize EGL");
                }
            }

            if (!Egl.ChooseConfig(display, configAttribs, configs, 1, configCount))
                throw new InvalidOperationException("Unable to choose configuration");

            holographicSpaceHandle = GCHandle.Alloc(holographicSpace);
            IntPtr holographicSpacePointer = Marshal.GetIUnknownForObject(holographicSpace);

            //PropertySet surfaceCreationProperties = new PropertySet
            //{
            //    { "EGLNativeWindowTypeProperty", (IntPtr)holographicSpaceHandle }
            //};

            PropertySet surfaceCreationProperties = new PropertySet();
            surfaceCreationProperties.Add("EGLNativeWindowTypeProperty", holographicSpacePointer);

            //if (stationaryReferenceFrame != null)
            //{
            //    stationaryReferenceFrameHandle = GCHandle.Alloc(stationaryReferenceFrame);
            //    surfaceCreationProperties.Add("EGLBaseCoordinateSystemProperty", (IntPtr)stationaryReferenceFrameHandle);
            //}

            GCHandle surfaceCreationPropertiesHandle = GCHandle.Alloc(surfaceCreationProperties);

            IntPtr surfaceCreationPropertiesPointer = Marshal.GetIUnknownForObject(surfaceCreationProperties);

            if ((surface = Egl.CreateWindowSurface(display, configs[0], surfaceCreationPropertiesPointer, surfaceAttribs)) == IntPtr.Zero)
                throw new InvalidOperationException("Unable to create EGL fullscreen surface");

            if ((context = Egl.CreateContext(display, configs[0], IntPtr.Zero, contextAttribs)) == IntPtr.Zero)
                throw new InvalidOperationException("Unable to create context");

            return context;
        }
    }
}
