using System.Drawing;
using System.Numerics;
using Alex.Engine;
using Alex.Gamestates;
using Alex.Graphics;
using Alex.Utils;
using Veldrid;

namespace Alex.Rendering.UI
{
    public class Logo : UIComponent
    {
        private static readonly string[] AlexLogo =
        {
            "               AAA                lllllll                                        ",
            "              A:::A               l:::::l                                        ",
            "             A:::::A              l:::::l                                        ",
            "            A:::::::A             l:::::l                                        ",
            "           A:::::::::A             l::::l      eeeeeeeeeeee   xxxxxxx      xxxxxxx",
            "          A:::::A:::::A            l::::l    ee::::::::::::ee  x:::::x    x:::::x ",
            "         A:::::A A:::::A           l::::l   e::::::eeeee:::::ee x:::::x  x:::::x  ",
            "        A:::::A   A:::::A          l::::l  e::::::e     e:::::e  x:::::xx:::::x   ",
            "       A:::::A     A:::::A         l::::l  e:::::::eeeee::::::e   x::::::::::x    ",
            "      A:::::AAAAAAAAA:::::A        l::::l  e:::::::::::::::::e     x::::::::x	    ",
            "     A:::::::::::::::::::::A       l::::l  e::::::eeeeeeeeeee      x::::::::x     ",
            "    A:::::AAAAAAAAAAAAA:::::A      l::::l  e:::::::e              x::::::::::x    ",
            "   A:::::A             A:::::A    l::::::l e::::::::e            x:::::xx:::::x   ",
            "  A:::::A               A:::::A   l::::::l  e::::::::eeeeeeee   x:::::x  x:::::x  ",
            " A:::::A                 A:::::A  l::::::l   ee:::::::::::::e  x:::::x    x:::::x ",
            "AAAAAAA                   AAAAAAA llllllll     eeeeeeeeeeeeee xxxxxxx      xxxxxxx"
        };

        private bool _doPlus = true;

        private float _scale = 1.0f;
        private string _splashText = "";

	    private Texture WoodTexture { get; set; } = null;
	    private Texture GrassTexture { get; set; } = null;
	    public bool DrawMotd { get; set; } = true;
	    public bool Center { get; set; } = false;

		private Alex Game { get; }
        public Logo(Alex alex)
        {
	        Game = alex;
            if (_splashText == "") _splashText = SplashTexts.GetSplashText();
        }

        private Vector2 CenterScreen(GraphicsDevice graphics)
        {
		//	return new Vector2(0, 0);
            return new Vector2(Game.Viewport.X + (( Game.Viewport.Width)/2f),
	            Game.Viewport.Y + (( Game.Viewport.Height)/2f));
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
        }

        public override void Render(RenderArgs args)
        {
	        if (WoodTexture == null || GrassTexture == null)
	        {
		        WoodTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.wood);
		        GrassTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.grass);
			}

            args.SpriteBatch.Begin(args.Commands);
            Vector2 centerScreen = CenterScreen(args.GraphicsDevice);

	        int totalX = 0;
	        int height = 0;
            var x = 0;
            var y = 25;
	        if (Center)
	        {
		        y = (int) (centerScreen.Y - (AlexLogo.Length * 8));

				Location.Y = y;
		        Location.X = centerScreen.X;
	        }
            foreach (var line in AlexLogo)
            {
                foreach (var i in line)
                {
                    float renderX = centerScreen.X - ((line.Length * 8 / 2) - x);

                    if (i == ':')
                    {
                        args.SpriteBatch.Draw(WoodTexture, new Vector2(renderX, y));

                    }
                    else if (i != ' ')
                    {
                        args.SpriteBatch.Draw(GrassTexture, new Vector2(renderX, y));
                    }

                    x += 8;
				}

				totalX = x;

				y += 8;
	            height += 8;

				x = 0;
	          
			}

	        Size.Y = height;

	        if (DrawMotd)
	        {
		        float dt = (float) args.GameTime.ElapsedGameTime.TotalSeconds;
		        if (_doPlus)
		        {
			        if (_scale < 1.25f)
			        {
				        _scale += 1f * dt;
			        }
			        else
			        {
				        _doPlus = false;
					}
				}
		        else
		        {
			        if (_scale > 0.75f)
			        {
				        _scale -= 1f * dt;
					}
			        else
			        {
				        _doPlus = true;
			        }
		        }

		        try
		        {
			        var textSize = Alex.Font.MeasureString(_splashText);
			        args.SpriteBatch.DrawString(Alex.Font, _splashText, new Vector2(centerScreen.X + (totalX / 2f), y /*+ (textSize.X / 2)*/), Color.Gold,
				        -0.6f,
				        textSize /2f, 
				        new Vector2(_scale, _scale));
		        }
		        catch
		        {
			        args.SpriteBatch.DrawString(Alex.Font, "Free bugs for everyone!", new Vector2(centerScreen.X + 186, 140),
				        Color.Gold,
				        -0.6f, new Vector2(),
				        new Vector2(_scale, _scale));
		        }
	        }

	        args.SpriteBatch.End();
        }
    }
}
