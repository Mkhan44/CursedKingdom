using System.Text;

namespace PurrNet.Modules
{
    public readonly struct GameObjectFrameworkPiece
    {
        public readonly PrefabPieceID pid;
        public readonly NetworkID id;
        public readonly int childCount;
        public readonly bool isActive;
        public readonly int[] inversedRelativePath;

        public GameObjectFrameworkPiece(PrefabPieceID pid, NetworkID id, int childCount, bool isActive,
            int[] path)
        {
            this.pid = pid;
            this.id = id;
            this.childCount = childCount;
            this.inversedRelativePath = path;
            this.isActive = isActive;
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append("GameObjectFrameworkPiece: { ");
            builder.Append("Pid: ");
            builder.Append(pid);
            builder.Append(", Nid: ");
            builder.Append(id);
            builder.Append(", childCount: ");
            builder.Append(childCount);
            builder.Append(", Path: ");
            for (int i = 0; i < inversedRelativePath.Length; i++)
            {
                builder.Append(inversedRelativePath[i]);
                if (i < inversedRelativePath.Length - 1)
                    builder.Append(" <- ");
            }

            builder.Append(" }");
            return builder.ToString();
        }
    }
}