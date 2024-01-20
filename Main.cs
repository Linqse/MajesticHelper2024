

using System.Reactive.Disposables;
using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using PoeShared.Modularity;

namespace EyeAuras.Web.Repl.Component;

public partial class Main : WebUIComponent {
    
    public Main(IAppArguments appArguments,
        IAuraTreeScriptingApi treeApi){
        ConfigPath = Path.Combine(appArguments.AppDataDirectory, "EyeSquad", $"Majestic-{treeApi.Aura.Id}.cfg");
        activeOrangeAnchors = new SerialDisposable().AddTo(Anchors);
        activeLumberAnchors = new SerialDisposable().AddTo(Anchors);
        activeMinerAnchors = new SerialDisposable().AddTo(Anchors);
        activeCareerAnchors = new SerialDisposable().AddTo(Anchors);
        activeFishAnchors = new SerialDisposable().AddTo(Anchors);
        activeCaptchaAnchors = new SerialDisposable().AddTo(Anchors);
        activeShroomsAnchors = new SerialDisposable().AddTo(Anchors);
    }
    private int DifY { get; set; }
    private static string ConfigPath { get; set; } 
    private Config _config = new Config();
    private IMLSearchTrigger MLGather => AuraTree.FindAuraByPath(@".\ML\Main").Triggers.Items.OfType<IMLSearchTrigger>().ElementAt(0);
    private IMLSearchTrigger MLFish => AuraTree.FindAuraByPath(@".\ML\Main").Triggers.Items.OfType<IMLSearchTrigger>().ElementAt(1);
    private IMLSearchTrigger MLCaptcha => AuraTree.FindAuraByPath(@".\ML\Main").Triggers.Items.OfType<IMLSearchTrigger>().ElementAt(2);
    private IMLSearchTrigger MLLumber => AuraTree.FindAuraByPath(@".\ML\Main").Triggers.Items.OfType<IMLSearchTrigger>().ElementAt(3);
    
    private IImageSearchTrigger ImagebuttonE => AuraTree.FindAuraByPath(@".\Images\E").Triggers.Items.OfType<IImageSearchTrigger>().First();
    private IImageSearchTrigger ImageLkm => AuraTree.FindAuraByPath(@".\Images\Helper").Triggers.Items.OfType<IImageSearchTrigger>().ElementAt(0);
    private IImageSearchTrigger ImageSuccess => AuraTree.FindAuraByPath(@".\Images\Helper").Triggers.Items.OfType<IImageSearchTrigger>().ElementAt(1);
    private IImageSearchTrigger ImageError => AuraTree.FindAuraByPath(@".\Images\Helper").Triggers.Items.OfType<IImageSearchTrigger>().ElementAt(2);
    private IImageSearchTrigger ImageWarn => AuraTree.FindAuraByPath(@".\Images\Helper").Triggers.Items.OfType<IImageSearchTrigger>().ElementAt(3);
    private IImageSearchTrigger ImageAD => AuraTree.FindAuraByPath(@".\Images\Helper").Triggers.Items.OfType<IImageSearchTrigger>().ElementAt(4);
    private IImageSearchTrigger ImageRange => AuraTree.FindAuraByPath(@".\Images\Range").Triggers.Items.OfType<IImageSearchTrigger>().First();
    
    private ITextSearchTrigger TextSearch =>  AuraTree.FindAuraByPath(@".\Images\TextSearch").Triggers.Items.OfType<ITextSearchTrigger>().First();
    

    private IWinExistsTrigger WinExists => AuraTree.FindAuraByPath(@".\WinExists").Triggers.Items.OfType<IWinExistsTrigger>().First();
    
    
    protected override async Task HandleAfterFirstRender()
    {
        
            this.WhenAnyValue(x => x.Orange)
                .Where(x => x)
                .ObserveOn(Scheduler.Default)
                .SubscribeAsync(_ => StartOrange())
                .AddTo(Anchors);
            
            
            this.WhenAnyValue(x => x.Lumber)
                .Where(x => x)
                .ObserveOn(Scheduler.Default)
                .SubscribeAsync(_ => StartLumber())
                .AddTo(Anchors);
            
            this.WhenAnyValue(x => x.Miner)
                .Where(x => x)
                .ObserveOn(Scheduler.Default)
                .SubscribeAsync(_ => StartMiner())
                .AddTo(Anchors);
            
            this.WhenAnyValue(x => x.Career)
                .Where(x => x)
                .ObserveOn(Scheduler.Default)
                .SubscribeAsync(_ => StartCareer())
                .AddTo(Anchors);
            
            this.WhenAnyValue(x => x.Fish)
                .Where(x => x)
                .ObserveOn(Scheduler.Default)
                .SubscribeAsync(_ => StartFish())
                .AddTo(Anchors);
            this.WhenAnyValue(x => x.Captcha)
                .Where(x => x)
                .ObserveOn(Scheduler.Default)
                .SubscribeAsync(_ => StartCaptcha())
                .AddTo(Anchors);
            
            this.WhenAnyValue(x => x.Shrooms)
                .Where(x => x)
                .ObserveOn(Scheduler.Default)
                .SubscribeAsync(_ => StartShrooms())
                .AddTo(Anchors);

            

            LoadConfig();
    }


    private async Task SaveScreen(Image<Bgr,byte> img)
    {
        string directoryPath = @"J:\FishScreens";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fileName = $"Screen_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        string fullPath = Path.Combine(directoryPath, fileName);

        await Task.Run(() => img.Save(fullPath));
    }
    
    private void SaveConfig()
    {

        string json = JsonConvert.SerializeObject(_config);
        string directoryPath = Path.GetDirectoryName(ConfigPath);
        
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using (StreamWriter streamWriter = new StreamWriter(ConfigPath))
        using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
        {
            jsonWriter.Formatting = Formatting.Indented; // Устанавливаем форматирование для отступов

            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(jsonWriter, _config);
        }
        
    }
    
    
    private void LoadConfig()
    {
        if (File.Exists(ConfigPath))
        {
            string json = File.ReadAllText(ConfigPath);
            _config = JsonConvert.DeserializeObject<Config>(json);
        }
        else
        {
            _config = new Config();
        }

        
    }
    
}