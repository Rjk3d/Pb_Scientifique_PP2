using System;
using System.Collections.Generic;

namespace TourneeFutee
{
    public class Tour
    {
        // TODO : ajouter tous les attributs que vous jugerez pertinents 
        private List<(string source, string destination)> segments;
        private float cost;

        public Tour()
        {
            segments = new List<(string source, string destination)>();
            cost = 0;
        }

        // propriétés

        // Coût total de la tournée
        public float Cost
        {
            get { return cost; }    // TODO : implémenter
        }

        // Nombre de trajets dans la tournée
        public int NbSegments
        {
            get { return segments.Count; }    // TODO : implémenter
        }


        // Renvoie vrai si la tournée contient le trajet `source`->`destination`
        public bool ContainsSegment((string source, string destination) segment)
        {
            return segments.Contains(segment);   // TODO : implémenter 
        }


        // Affiche les informations sur la tournée : coût total et trajets
        public void Print()
        {
            Console.WriteLine("Coût total : " + cost);
            Console.WriteLine("Trajets :");
            foreach ((string source, string destination) segment in segments)
            {
                Console.WriteLine(segment.source + " -> " + segment.destination);
            }
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

        public void AddSegment(string source, string destination, float segmentCost)
        {
            segments.Add((source, destination));
            cost += segmentCost;
        }
    }
}