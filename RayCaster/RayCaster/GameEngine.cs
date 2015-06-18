using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RayCasterGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameEngine : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        int[] _buffer;
        Texture2D _outputTexture;


        private const int ScreenWidth = 1024;
        private const int ScreenHeight = 768;

        private const int RayCastRenderWidth = 1024;
        private const int RayCastRenderHeight = 768;

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
            // TODO: Add your initialization logic here

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
            _buffer = new int[RayCastRenderWidth * RayCastRenderHeight];
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

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

            for (int y = 0; y < RayCastRenderHeight; y++)
            {
                for (int x = 0; x < RayCastRenderWidth; x++)
                {
                    byte a = 0xFF;
                    byte b = (byte)(x % 256);
                    byte g = (byte)(y % 256);
                    byte r = (byte)(256 * ((gameTime.TotalGameTime.Seconds % 10) / 10.0));

                    _buffer[y * RayCastRenderWidth + x] = a << 24 | b << 16 | g << 8 | r;
                }
            }

            _outputTexture.SetData(_buffer);

            _spriteBatch.Draw(
                texture: _outputTexture,
                destinationRectangle: new Rectangle(
                    x: (ScreenWidth - RayCastRenderWidth) / 2,
                    y: 0,
                    width: RayCastRenderWidth,
                    height: RayCastRenderHeight),
                color: Color.White);

            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
