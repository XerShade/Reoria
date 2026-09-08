using Gum;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using Button = Gum.Forms.Controls.Button;
using CheckBox = Gum.Forms.Controls.CheckBox;
using GameBase = Microsoft.Xna.Framework.Game;
using ListBox = Gum.Forms.Controls.ListBox;
using Slider = Gum.Forms.Controls.Slider;
using TextBox = Gum.Forms.Controls.TextBox;

namespace Reoria.Client.Interface.Services;

/// <summary>
/// Game loop phase participant that provides a graphical user interface using Gum.
/// </summary>
/// <remarks>
/// This is a game loop phase participant with full DI support - constructor dependencies are resolved from the container.
/// </remarks>
public class GumFormsService : IGameInitialize, IGameVariableUpdate, IGameRender
{
    public string Name
        => "Gum Forms Service";

    public string Description
        => "Provides a graphical user interface using Gum during game loop.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All & ~Platform.Server;

    protected GumService GumUI { get; init; } = GumService.Default;

    public virtual void OnInitializeGame(GameBase game)
    {
        this.GumUI.Initialize(game, Gum.Forms.DefaultVisualsVersion.Newest);

        ListBox listBox = new ListBox();
        this.GumUI.Root.AddChild(listBox.Visual);
        listBox.X = 50;
        listBox.Y = 50;
        listBox.Width = 400;
        listBox.Height = 200;

        Button button = new Button();
        this.GumUI.Root.AddChild(button.Visual);
        button.X = 460;
        button.Y = 50;
        button.Width = 200;
        button.Height = 40;
        button.Text = "Add to ListBox";
        button.Click += (s, e) =>
        {
            string newItem = $"Item @ {DateTime.Now}";
            listBox.Items.Add(newItem);
            listBox.ScrollIntoView(newItem);
        };

        CheckBox checkBox = new CheckBox();
        this.GumUI.Root.AddChild(checkBox.Visual);
        checkBox.X = 460;
        checkBox.Y = 125;
        checkBox.Text = "Checkbox";
        checkBox.Checked += (_, _) => Console.WriteLine($"IsChecked:{checkBox.IsChecked}");
        checkBox.Unchecked += (_, _) => Console.WriteLine($"IsChecked:{checkBox.IsChecked}");

        Slider slider = new Slider();
        this.GumUI.Root.AddChild(slider.Visual);
        slider.X = 460;
        slider.Y = 160;
        slider.Minimum = 0;
        slider.Maximum = 30;
        slider.TicksFrequency = 1;
        slider.IsSnapToTickEnabled = true;
        slider.Width = 250;
        slider.ValueChanged += (_, _) =>
            Console.WriteLine($"Value: {slider.Value}");
        slider.ValueChangeCompleted += (_, _) =>
            Console.WriteLine($"Finished setting Value: {slider.Value}");

        TextBox textBox = new TextBox();
        this.GumUI.Root.AddChild(textBox.Visual);
        textBox.X = 50;
        textBox.Y = 260;
        textBox.Width = 200;
        textBox.Height = 34;
        textBox.Placeholder = "Placeholder Text...";

        TextBox textBox2 = new TextBox();
        this.GumUI.Root.AddChild(textBox2.Visual);
        textBox2.X = 50;
        textBox2.Y = 300;
        textBox2.Width = 200;
        textBox2.Height = 34;
        textBox2.Placeholder = "Placeholder Text...";
    }

    public void OnVariableUpdate(GameTime gameTime)
        => this.GumUI.Update(gameTime);

    public void OnRender(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        => this.GumUI.Draw();
}