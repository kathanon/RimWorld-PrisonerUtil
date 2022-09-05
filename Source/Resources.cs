using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PrisonerUtil {
    public static class Resources {
        public static readonly Texture2D PrisonerIcon =
            ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners");
    }
}
