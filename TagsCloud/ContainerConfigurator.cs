using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Autofac;
using Autofac.Core;
using TagsCloud.BoringWordsDetectors;
using TagsCloud.CloudRenderers;
using TagsCloud.ColorSelectors;
using TagsCloud.PointsLayouts;
using TagsCloud.StatisticProviders;
using TagsCloud.WordLayouters;
using TagsCloud.WordReaders;
using TagsCloud.WordSelectors;
using IContainer = Autofac.IContainer;

namespace TagsCloud
{
    public static class ContainerConfigurator
    {
        private static readonly Dictionary<string, IWordSelector> WordSelectors = new Dictionary<string, IWordSelector>
        {
            ["All"] = new AllWordSelector(),
        };
        
        private static readonly Dictionary<string, IBoringWordsDetector> BoringWordsDetectors = new Dictionary<string, IBoringWordsDetector>
        {
            ["By Collection"] = new ByCollectionBoringWordsDetector(),
        };
        
        private static readonly Dictionary<string, IPointsLayout> PointsLayouts = new Dictionary<string, IPointsLayout>
        {
            ["Spiral"] = new SpiralPoints(),
            ["Square"] = new SquarePoints(),
        };
        
        private static readonly Dictionary<string, Type> ColorSelectors = new Dictionary<string, Type>
        {
            ["Random"] = typeof(RandomColorSelector),
            ["Cyclic"] = typeof(CyclicColorSelector),
        };
        
        public static IContainer Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(ReadType<string>("File path"));
            builder.RegisterInstance(ReadInterface("Words selector", WordSelectors)).As<IWordSelector>();
            builder.RegisterType<RegexWordReader>().As<IWordReader>();

            builder.RegisterInstance(ReadInterface("Boring words detector", BoringWordsDetectors))
                .As<IBoringWordsDetector>();
            builder.RegisterType<StatisticProvider>().As<IStatisticProvider>();

            builder.RegisterInstance(ReadFont());
            builder.RegisterInstance(ReadInterface("Points layout", PointsLayouts)).As<IPointsLayout>();
            builder.RegisterType<WordLayouter>().SingleInstance().As<IWordLayouter>();

            builder.RegisterInstance(ReadColors());
            builder.RegisterType(ReadInterface("Color selector", ColorSelectors)).SingleInstance().As<IColorSelector>();
            
            var width = ReadType<int>("Image width");
            var height = ReadType<int>("Image height");
            builder.RegisterType<CloudRenderer>()
                .As<ICloudRenderer>()
                .WithParameters(new Parameter[]
                {
                    new NamedParameter("width", width),
                    new NamedParameter("height", height), 
                });
            
            return builder.Build();
        }

        private static T ReadType<T>(string parameterName)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            while (true)
            {
                Console.Write($"{parameterName}: ");
                var input = Console.ReadLine();
                try
                {
                    var result = converter.ConvertFrom(input);
                    return (T)result;
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Incorrect string");
                }
            }
        }
        
        private static T ReadInterface<T>(string parameterName, Dictionary<string, T> options)
        {
            Console.WriteLine($"{parameterName}: ");
            var selector = options.ToArray();
            for (var i = 0; i < selector.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {selector[i].Key}");
            }
            while (true)
            {
                var input = Console.ReadLine();
                try
                {
                    var index = int.Parse(input);
                    if(1 > index || index > selector.Length) throw new ArgumentException();
                    return selector[index - 1].Value;
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"Input number from 1 to {selector.Length}");
                }
            }
        }
        
        private static FontFamily ReadFont()
        {
            while (true)
            {
                Console.Write("Font: ");
                var input = Console.ReadLine();
                try
                {
                    var font = new FontFamily(input);
                    return font;
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"{input} doesn't exist");
                }
            }
        }

        private static Color[] ReadColors()
        {
            Console.WriteLine("Write colors. To continue press Enter");
            var colors = new List<Color>();
            while (true)
            {
                var input = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(input)) break;
                var color = Color.FromName(input);
                if(color.IsKnownColor)
                    colors.Add(Color.FromName(input));
                else
                    Console.WriteLine($"Unknown color {input}");
            }

            return colors.Count == 0 ? new[] {Color.Black} : colors.ToArray();
        }
    }
}