﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace ShaderTestMod
{
    public class ShaderTest : ModSystem
    {
        ICoreClientAPI capi;
        OrthoRenderer[] orthoRenderers;
        public static float[] controls;

        readonly string[] uniforms = new string[]
        {
            "iTime", "iResolution",
            "iMouse", "iCamera",
            "iSunPos", "iMoonPos",
            "iMoonPhase", "iPlayerPosition",
            "iTemperature", "iRainfall",
            "iControls1", "iControls2",
            "iControls3", "iControls4",
            "iCurrentHealth", "iMaxHealth"
        };

        readonly string[] orthoShaderKeys = new string[] { "basicshader" };

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;
            api.Event.PlayerJoin += StartShade;
        }

        public void StartShade(IPlayer player)
        {
            controls = ControlsAsFloats(player.Entity);
            capi.Event.RegisterGameTickListener(dt => controls = ControlsAsFloats(player.Entity), 30);

            capi.Event.ReloadShader += LoadShaders;
            LoadShaders();

            for (int i = 0; i < orthoRenderers.Length; i++)
            {
                capi.Event.RegisterRenderer(orthoRenderers[i], EnumRenderStage.Ortho, orthoShaderKeys[i]);
            }
        }

        public bool LoadShaders()
        {
            List<OrthoRenderer> rendererers = new List<OrthoRenderer>();

            for (int i = 0; i < orthoShaderKeys.Length; i++)
            {
                IShaderProgram shader = capi.Shader.NewShaderProgram();
                int program = capi.Shader.RegisterFileShaderProgram(orthoShaderKeys[i], shader);
                shader = capi.Render.GetShader(program);
                shader.PrepareUniformLocations(uniforms);
                shader.Compile();

                OrthoRenderer renderer = new OrthoRenderer(capi, shader);

                if (orthoRenderers != null)
                {
                    orthoRenderers[i].Dispose();
                    capi.Event.UnregisterRenderer(orthoRenderers[i], EnumRenderStage.Ortho);

                    orthoRenderers[i].prog = shader;
                    capi.Event.RegisterRenderer(orthoRenderers[i], EnumRenderStage.Ortho, orthoShaderKeys[i]);
                }
                rendererers.Add(renderer);
            }
            orthoRenderers = rendererers.ToArray();

            return true;
        }

        public float[] ControlsAsFloats(EntityPlayer entity)
        {
            EntityControls c = entity.Controls;
            return new float[]
            {
                c.Backward ? 1 : 0, //0
                c.Down ? 1 : 0, //1
                c.FloorSitting ? 1 : 0, //2
                c.Forward ? 1 : 0, //3
                c.Jump ? 1 : 0, //4
                c.Left ? 1 : 0, //5
                c.LeftMouseDown ? 1 : 0, //6
                c.Right ? 1 : 0, //7
                c.RightMouseDown ? 1 : 0, //8
                c.Sitting ? 1 : 0, //9
                c.Sneak ? 1 : 0, //10
                c.Sprint ? 1 : 0, //11
                c.TriesToMove ? 1 : 0, // 12
                c.Up ? 1 : 0, //13
            };
        }
    }

    public class OrthoRenderer : IRenderer
    {
        MeshRef quadRef;
        ICoreClientAPI capi;
        public IShaderProgram prog;
        ITreeAttribute healthTree;

        public Matrixf ModelMat = new Matrixf();

        public double RenderOrder => 1.1;

        public int RenderRange => 1;

        public OrthoRenderer(ICoreClientAPI api, IShaderProgram prog)
        {
            this.prog = prog;
            capi = api;

            MeshData quadMesh = QuadMeshUtil.GetCustomQuadModelData(-1, -1, 0, 2, 2);
            quadMesh.Rgba = null;

            quadRef = capi.Render.UploadMesh(quadMesh);
            healthTree = capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("health");
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (prog.Disposed) return;

            BlockPos pos = capi.World.Player.Entity.Pos.AsBlockPos;
            IShaderProgram curShader = capi.Render.CurrentActiveShader;
            curShader.Stop();

            prog.Use();
            
            capi.Render.GlToggleBlend(true);
            prog.Uniform("iTime", capi.World.ElapsedMilliseconds / 500f);
            prog.Uniform("iResolution", new Vec2f(capi.Render.FrameWidth, capi.Render.FrameHeight));
            prog.Uniform("iMouse", new Vec2f(capi.Input.MouseX, capi.Input.MouseY));
            prog.Uniform("iCamera", new Vec2f(capi.World.Player.CameraPitch, capi.World.Player.CameraYaw));
            prog.Uniform("iSunPos", capi.World.Calendar.SunPosition);
            prog.Uniform("iMoonPos", capi.World.Calendar.MoonPosition);
            prog.Uniform("iMoonPhase", (float)capi.World.Calendar.MoonPhaseExact);
            prog.Uniform("iPlayerPosition", capi.World.Player.Entity.LocalPos.XYZ.ToVec3f());
            prog.Uniform("iTemperature", capi.World.BlockAccessor.GetClimateAt(pos).Temperature);
            prog.Uniform("iRainfall", capi.World.BlockAccessor.GetClimateAt(pos).Rainfall);
            prog.Uniform("iCurrentHealth", (float)healthTree.TryGetFloat("currenthealth"));
            prog.Uniform("iMaxHealth", (float)healthTree.TryGetFloat("maxhealth"));

            float[] c = ShaderTest.controls;
            prog.Uniform("iControls1", new Vec4f(c[0], c[1], c[2], c[3]));
            prog.Uniform("iControls2", new Vec4f(c[4], c[5], c[6], c[7]));
            prog.Uniform("iControls3", new Vec4f(c[8], c[9], c[10], c[11]));
            prog.Uniform("iControls4", new Vec2f(c[12], c[13]));

            capi.Render.RenderMesh(quadRef);
            prog.Stop();
            curShader.Use();
        }

        public void Dispose()
        {
        }
    }
}
