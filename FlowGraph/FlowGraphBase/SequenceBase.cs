﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using FlowGraphBase.Node;
using FlowGraphBase.Process;

namespace FlowGraphBase
{
    /// <summary>
    /// Manage a sequence of nodes.
    /// </summary>
    public class SequenceBase : INotifyPropertyChanged
    {
        static int _newId;

        protected readonly Dictionary<int, SequenceNode> SequenceNodes = new Dictionary<int, SequenceNode>();

        private string _name, _description;

        /// <summary>
        /// Gets/Sets
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (string.Equals(_name, value) == false)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Gets/Sets
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (string.Equals(_description, value) == false)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        /// <summary>
        /// Gets
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets
        /// </summary>
        public IEnumerable<SequenceNode> Nodes => SequenceNodes.Values.ToArray();

        /// <summary>
        /// Gets
        /// </summary>
        public int NodeCount => SequenceNodes.Values.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected SequenceBase(string name)
        {
            Name = name;
            Id = _newId++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        protected SequenceBase(XmlNode node)
        {
            Load(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SequenceNode GetNodeById(int id)
        {
            return SequenceNodes[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(SequenceNode node)
        {
            SequenceNodes.Add(node.Id, node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(SequenceNode node)
        {
            SequenceNodes.Remove(node.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveAllNodes()
        {
            SequenceNodes.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryStack"></param>
        public void AllocateAllVariables(MemoryStack memoryStack)
        {
            foreach (VariableNode varNode in SequenceNodes.Select(pair => pair.Value).OfType<VariableNode>())
            {
                varNode.Allocate(memoryStack);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetNodes()
        {
            foreach (var pair in SequenceNodes)
            {
                pair.Value.Reset();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="param"></param>
        public void OnEvent(ProcessingContext context, Type type, int index, object param)
        {
            //_MustStop = false;

            foreach (var eventNode in SequenceNodes.Select(pair => pair.Value as EventNode)
                .Where(node => node != null
                       && node.GetType() == type))
            {
                eventNode.Triggered(context, index, param);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public virtual void Load(XmlNode node)
        {
            Id = int.Parse(node.Attributes["id"].Value);
            if (_newId <= Id) _newId = Id + 1;
            Name = node.Attributes["name"].Value;
            Description = node.Attributes["description"].Value;

            foreach (XmlNode nodeNode in node.SelectNodes("NodeList/Node"))
            {
                int versionNode = int.Parse(nodeNode.Attributes["version"].Value);

                SequenceNode seqNode = SequenceNode.CreateNodeFromXml(nodeNode);

                if (seqNode != null)
                {
                    AddNode(seqNode);
                }
                else
                {
                    throw new InvalidOperationException("Can't create SequenceNode from xml " +
                                                        $"id={nodeNode.Attributes["id"].Value}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void ResolveNodesLinks(XmlNode node)
        {
            if (node == null) throw new ArgumentNullException("XmlNode");

            XmlNode connectionListNode = node.SelectSingleNode("ConnectionList");

            foreach (var sequenceNode in SequenceNodes)
            {
                sequenceNode.Value.ResolveLinks(connectionListNode, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public virtual void Save(XmlNode node)
        {
            const int version = 1;

            XmlNode graphNode = node.OwnerDocument.CreateElement("Graph");
            node.AppendChild(graphNode);

            graphNode.AddAttribute("version", version.ToString());
            graphNode.AddAttribute("id", Id.ToString());
            graphNode.AddAttribute("name", Name);
            graphNode.AddAttribute("description", Description);

            //save all nodes
            XmlNode nodeList = node.OwnerDocument.CreateElement("NodeList");
            graphNode.AppendChild(nodeList);
            //save all connections
            XmlNode connectionList = node.OwnerDocument.CreateElement("ConnectionList");
            graphNode.AppendChild(connectionList);

            foreach (var pair in SequenceNodes)
            {
                XmlNode nodeNode = node.OwnerDocument.CreateElement("Node");
                nodeList.AppendChild(nodeNode);
                pair.Value.Save(nodeNode);
                pair.Value.SaveConnections(connectionList);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
