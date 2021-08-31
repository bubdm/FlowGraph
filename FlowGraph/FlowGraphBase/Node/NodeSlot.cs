﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using FlowGraphBase.Process;

namespace FlowGraphBase.Node
{
    /// <summary>
    /// 
    /// </summary>
    public enum SlotAvailableFlag
    {
        None = 0,
        NodeIn = 1 << 1,
        NodeOut = 1 << 2,
        VarOut = 1 << 3,
        VarIn = 1 << 4,

        DefaultFlagEvent = NodeOut | VarOut,
        DefaultFlagVariable = VarIn | VarOut,
        DefaultFlagAction = NodeIn | NodeOut,
        All = NodeIn | NodeOut | VarIn | VarOut
    }

    /// <summary>
    /// 
    /// </summary>
    public enum SlotType
    {
        NodeIn,
        NodeOut,
        VarOut,
        VarIn,
        VarInOut, // special case for variable node which can be in/out at the same time
    }

    /// <summary>
    /// A node slot contains all links to the other nodes
    /// </summary>
    public class NodeSlot : INotifyPropertyChanged
    {
        public event EventHandler Activated;

        private string _Text;
        private Type _VariableType;
        private VariableControlType _ControlType;

        public int ID { get; }
        public SequenceNode Node { get; }
        public virtual SlotType ConnectionType { get; }
        public object Tag { get; }
        public List<NodeSlot> ConnectedNodes { get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Text
        {
            get => _Text;
            set 
            {
                if (string.Equals(_Text, value) == false)
                {
                    _Text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Type VariableType
        {
            get => _VariableType;
            set
            {
                if (_VariableType != value)
                {
                    _VariableType = value;
                    OnPropertyChanged("VariableType");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual VariableControlType ControlType
        {
            get => _ControlType;
            set
            {
                if (_ControlType != value)
                {
                    _ControlType = value;
                    OnPropertyChanged("ControlType");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId_"></param>
        /// <param name="node_"></param>
        /// <param name="connectionType_"></param>
        /// <param name="controlType_"></param>
        /// <param name="tag_"></param>
        protected NodeSlot(int slotId_, SequenceNode node_, SlotType connectionType_,
            VariableControlType controlType_ = VariableControlType.ReadOnly,
            object tag_ = null)
        {
            ConnectedNodes = new List<NodeSlot>();

            ID = slotId_;
            Node = node_;
            ConnectionType = connectionType_;
            ControlType = controlType_;
            Tag = tag_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId_"></param>
        /// <param name="node_"></param>
        /// <param name="text_"></param>
        /// <param name="connectionType_"></param>
        /// <param name="type_"></param>
        /// <param name="controlType_"></param>
        /// <param name="tag_"></param>
        public NodeSlot(int slotId_, SequenceNode node_, string text_,
            SlotType connectionType_, Type type_ = null,
            VariableControlType controlType_ = VariableControlType.ReadOnly,
            object tag_ = null) :
            this(slotId_, node_, connectionType_, controlType_, tag_)
        {
            Text = text_;
            VariableType = type_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dst_"></param>
        public bool ConnectTo(NodeSlot dst_)
        {
            if (dst_.Node == Node)
            {
                throw new InvalidOperationException("Try to connect itself");
            }

            foreach (NodeSlot s in ConnectedNodes)
            {
                if (s.Node == dst_.Node) // already connected
                {
                    return true;
                    //throw new InvalidOperationException("");
                }
            }

            switch (ConnectionType)
            {
                case SlotType.NodeIn:
                case SlotType.NodeOut:
                    if ((dst_.Node is VariableNode))
                    {
                        return false;
                    }
                    break;

                case SlotType.VarIn:
                case SlotType.VarOut:
                case SlotType.VarInOut:
                    if ((dst_.Node is VariableNode) == false
                        && (dst_ is NodeSlotVar) == false)
                    {
                        return false;
                    }
                    break;
            }

            ConnectedNodes.Add(dst_);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot_"></param>
        public bool DisconnectFrom(NodeSlot slot_)
        {
            ConnectedNodes.Remove(slot_);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveAllConnections()
        {
            ConnectedNodes.Clear();
        }

        /// <summary>
        /// Used to activate the nodes in the next step, see Sequence.Run()
        /// </summary>
        /// <param name="context_"></param>
        public void RegisterNodes(ProcessingContext context_)
        {
            foreach (NodeSlot slot in ConnectedNodes)
            {
                if (slot.Node is ActionNode)
                {
                    context_.RegisterNextExecution(slot);
                }
            }

            if (Activated != null)
            {
                Activated.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        public virtual void Save(XmlNode node_)
        {
            const int version = 1;
            node_.AddAttribute("version", version.ToString());
            node_.AddAttribute("index", ID.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        public virtual void Load(XmlNode node_)
        {
            int version = int.Parse(node_.Attributes["version"].Value);
            //Don't load Id, it is set manually inside the constructor
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// A node slot contains all links to the other nodes
    /// </summary>
    public class NodeSlotVar : NodeSlot
    {
        private readonly ValueContainer _Value;
        private readonly bool _SaveValue;

        /// <summary>
        /// Used as nested link with a variable node
        /// </summary>
        public object Value
        {
            get => _Value.Value;
            set { _Value.Value = value; OnPropertyChanged("Value"); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId_"></param>
        /// <param name="node_"></param>
        /// <param name="text_"></param>
        /// <param name="connectionType_"></param>
        /// <param name="type_"></param>
        /// <param name="controlType_"></param>
        /// <param name="tag_"></param>
        public NodeSlotVar(int slotId_, SequenceNode node_, string text_,
            SlotType connectionType_, Type type_ = null,
            VariableControlType controlType_ = VariableControlType.ReadOnly,
            object tag_ = null, bool saveValue_ = true) :
            base(slotId_, node_, text_, connectionType_, type_, controlType_, tag_)
        {
            _SaveValue = saveValue_;

            object val = null;

            if (type_ == typeof(bool))
            {
                val = true;
            }
            else if (type_ == typeof(sbyte)
                || type_ == typeof(char)
                || type_ == typeof(short)
                || type_ == typeof(int)
                || type_ == typeof(long)
                || type_ == typeof(byte)
                || type_ == typeof(ushort)
                || type_ == typeof(uint)
                || type_ == typeof(ulong)
                || type_ == typeof(float)
                || type_ == typeof(double))
            {
                val = Convert.ChangeType(0, type_);
            }
            else if (type_ == typeof(string))
            {
                val = string.Empty;
            }

            _Value = new ValueContainer(type_, val);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        public override void Save(XmlNode node_)
        {
            base.Save(node_);

            node_.AddAttribute("saveValue", _SaveValue.ToString());

            if (_SaveValue)
            {
                _Value.Save(node_);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node_"></param>
        public override void Load(XmlNode node_)
        {
            base.Load(node_);

            if (_SaveValue)
            {
                _Value.Load(node_);
            }
        }
    }

    /// <summary>
    /// Specific node for all nodes linked with a SequenceFunction.
    /// NodeFunctionSlot reflects all changes made in real time.
    /// </summary>
    public class NodeFunctionSlot : NodeSlotVar
    {
        private readonly SequenceFunctionSlot _FuncSlot;

        /// <summary>
        /// 
        /// </summary>
        public override string Text
        {
            get => _FuncSlot == null ? string.Empty : _FuncSlot.Name;
            set 
            {
                if (_FuncSlot != null)
                {
                    _FuncSlot.Name = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Type VariableType
        {
            get => _FuncSlot == null ? null : _FuncSlot.VariableType;
            set
            {
                if (_FuncSlot != null)
                {
                    _FuncSlot.VariableType = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotId_"></param>
        /// <param name="node_"></param>
        /// <param name="connectionType_"></param>
        /// <param name="slot_"></param>
        /// <param name="controlType_"></param>
        /// <param name="tag_"></param>
        /// <param name="saveValue_"></param>
        public NodeFunctionSlot(
            int slotId_, 
            SequenceNode node_, 
            SlotType connectionType_, 
            SequenceFunctionSlot slot_,
            VariableControlType controlType_ = VariableControlType.ReadOnly,
            object tag_ = null, 
            bool saveValue_ = true) :

                base(slotId_, 
                    node_, 
                    slot_.Name,
                    connectionType_, 
                    slot_.VariableType,
                    controlType_,
                    tag_, 
                    saveValue_)
        {
            _FuncSlot = slot_;
            _FuncSlot.PropertyChanged += OnFunctionSlotPropertyChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFunctionSlotPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    OnPropertyChanged("Text");
                    break;

                case "VariableType":
                    OnPropertyChanged("VariableType");
                    break;
                //IsArray
            }
        }
    }
}
