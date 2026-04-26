using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Tour
    {
        public List<(string source, string destination)> trajets { get; set; }

        private float cout;

        public Tour()
        {
            trajets = new List<(string, string)>();
            cout = 0;
        }
        public Tour(List<string> vertices, float cost)
        {
            trajets = new List<(string, string)>();
            cout = cost;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                trajets.Add((vertices[i], vertices[i + 1]));
            }
        }

        public float Cost
        {
            get { return cout; }
            set { cout = value; }
        }

        public int NbSegments
        {
            get { return trajets.Count; }
        }

        public bool ContainsSegment((string source, string destination) segment)
        {
            foreach (var t in trajets)
            {
                if (t.Item1 == segment.Item1 && t.Item2 == segment.Item2)
                {
                    return true;
                }
            }
            return false;
        }

        public void Print()
        {
            Console.WriteLine("Coût de la tournée : " + cout);
            Console.WriteLine("Trajets :");
            foreach (var t in trajets)
            {
                Console.WriteLine($"- {t.Item1} -> {t.Item2}");
            }
        }
        public List<string> Vertices
        {
            get
            {
                List<string> vertices = new List<string>();

                if (trajets.Count == 0)
                    return vertices;

                vertices.Add(trajets[0].source);

                foreach (var trajet in trajets)
                {
                    vertices.Add(trajet.destination);
                }

                return vertices;
            }
        }
    }
    }