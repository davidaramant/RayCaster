using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System;

namespace RayCasterGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public sealed class GameEngine : Game
    {
        // STUFF TO DO:
        // * Replace use of Task Parallel Library
        // ** Byte version of HSV Color?
        // ** HLSL shader?
        // ** System.Numerics.Vectors?
        // ** CUDAfx?
        // * Dynamic resolution (changing size of screen buffer at runtime)
        // ** Dynamic detail?  Cut resolution by 2, 4, etc?
        // * Full screen support
        // * Sprite rendering
        // * Weapon sprites
        // * Text rendering (bitmap font)
        // * Change view height (crouching/jumping)
        // * Change rendering height (y-shearing - look up/down)
        // * Taller levels
        // * Skyboxes
        // * View bobbing
        // * Movement momentum
        // * Variable floor/ceiling heights
        // * Why does it calculate texture offsets beyond the bounds sometimes??????
        // * "Light maps"
        // * Water effect?
        // * Doors
        // * Transparent glass
        // * Texture warping (Quake-style)


        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        ScreenBuffer _buffer;

        Texture2D _outputTexture;
        RayCaster _caster;

        private const int ScreenWidth = 1024;
        private const int ScreenHeight = 768;

        private const int RayCastRenderWidth = 1024/2;
        private const int RayCastRenderHeight = 768/2;

        public GameEngine()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = ScreenWidth,
                PreferredBackBufferHeight = ScreenHeight,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = true,
            };
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.TargetElapsedTime = System.TimeSpan.FromSeconds(1 / 60.0);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _outputTexture = new Texture2D(_graphics.GraphicsDevice, width: RayCastRenderWidth, height: RayCastRenderHeight);

            _buffer = new ScreenBuffer(RayCastRenderWidth, RayCastRenderHeight);
            
            var texturesToLoad = new[]
            {
                "BROWN96",
                "COMPTILE",
                "STARGR2",
                "STARTAN2",
                "TEKWALL1",

                "FLOOR0_1",
                "FLAT3",
            };

            var namedTextureResources = texturesToLoad.Select(name => Tuple.Create(name, Content.Load<Texture2D>("Textures/" + name)));

            var namedTextures = namedTextureResources.Select(namedResource => Texture.FromTextureResource(namedResource.Item1, namedResource.Item2));

            Content.Unload();

            var mapData = new MapData(namedTextures);

            _caster = new RayCaster(mapData);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Prevent contradictory inputs

            var inputs = MovementInputs.None;

            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                inputs |= MovementInputs.Forward;
            }
            else if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                inputs |= MovementInputs.Backward;
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                inputs |= MovementInputs.TurnLeft;
            }
            else if (keyboardState.IsKeyDown(Keys.Right))
            {
                inputs |= MovementInputs.TurnRight;
            }

            if (keyboardState.IsKeyDown(Keys.Q))
            {
                inputs |= MovementInputs.StrafeLeft;
            }
            else if (keyboardState.IsKeyDown(Keys.E))
            {
                inputs |= MovementInputs.StrafeRight;
            }

            _caster.Update(inputs, gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Immediate,
                blendState: BlendState.Opaque,
                samplerState: SamplerState.PointWrap,
                depthStencilState: DepthStencilState.None,
                rasterizerState: RasterizerState.CullNone);

            _buffer.Clear();
            _caster.Render(_buffer);
            _buffer.CopyToTexture(_outputTexture);

            _spriteBatch.Draw(
                texture: _outputTexture,
                destinationRectangle: new Rectangle(
                    x: 0,
                    y: 0,
                    width: ScreenWidth,
                    height: ScreenHeight),
                color: Color.White);

            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
