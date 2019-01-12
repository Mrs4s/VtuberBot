using System;
using System.Collections.Generic;
using System.Text;

namespace VtuberBot.Robots
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RobotCommandAttribute : Attribute
    {
        public int ProcessLength { get; }

        public string SubCommandName { get; }

        public int SubCommandOffset { get; }


        public RobotCommandAttribute(int processLength)
        {
            ProcessLength = processLength;
        }

        public RobotCommandAttribute(int processLength, int offset, string subCommandName) : this(processLength)
        {
            SubCommandName = subCommandName;
            SubCommandOffset = offset;
        }

        public RobotCommandAttribute(int offset, string subCommandName)
        {
            SubCommandName = subCommandName;
            SubCommandOffset = offset;
        }


    }
}
