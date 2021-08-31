﻿using System.Xml;

namespace FlowGraphBase.Node
{
    /// <summary>
    /// 
    /// </summary>
    public enum VariableControlType
    {
        Numeric,
        Selectable,
        Checkable,
        Text,
        ReadOnly,
        Custom
    }


    /// <summary>
    /// 
    /// </summary>
    public abstract partial class VariableNode
        : SequenceNode
    {
        /// <summary>
        /// 
        /// </summary>
        public override NodeType NodeType => NodeType.Variable;

        /// <summary>
        /// 
        /// </summary>
        public NodeSlot VariableSlot => _nodeSlots[0];

        /// <summary>
        /// 
        /// </summary>
        public override string Title => "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name_"></param>
        public VariableNode()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        public override void Save(XmlNode node_)
        {
            base.Save(node_);

            XmlNode valueNode = node_.OwnerDocument.CreateElement("Value");
            node_.AppendChild(valueNode);
            SaveValue(valueNode);
        }

        /// <summary>
        /// Load the value from XmlNode
        /// </summary>
        /// <param name="node_"></param>
        protected abstract object LoadValue(XmlNode node_);

        /// <summary>
        /// Save the value into a XmlNode
        /// </summary>
        /// <param name="node_"></param>
        protected abstract void SaveValue(XmlNode node_);
    }
}
