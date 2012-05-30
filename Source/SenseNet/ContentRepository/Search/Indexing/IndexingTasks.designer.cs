﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SenseNet.Search.Indexing
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Runtime.Serialization;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="SenseNetContentRepository")]
	public abstract partial class IndexingTasksDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertIndexingActivity(IndexingActivity instance);
    partial void UpdateIndexingActivity(IndexingActivity instance);
    partial void DeleteIndexingActivity(IndexingActivity instance);
    partial void InsertIndexingTask(IndexingTask instance);
    partial void UpdateIndexingTask(IndexingTask instance);
    partial void DeleteIndexingTask(IndexingTask instance);
    #endregion
		
		public IndexingTasksDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public IndexingTasksDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public IndexingTasksDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public IndexingTasksDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<IndexingActivity> IndexingActivities
		{
			get
			{
				return this.GetTable<IndexingActivity>();
			}
		}
		
		public System.Data.Linq.Table<IndexingTask> IndexingTasks
		{
			get
			{
				return this.GetTable<IndexingTask>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.IndexingActivity")]
	[global::System.Runtime.Serialization.DataContractAttribute()]
	public partial class IndexingActivity : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _IndexingActivityId;
		
		private int _IndexingTaskId;
		
		private IndexingActivityType _ActivityType;
		
		private int _NodeId;
		
		private int _VersionId;
		
		private System.Nullable<bool> _IsPublicValue;
		
		private System.Nullable<bool> _IsLastPublicValue;
		
		private System.Nullable<bool> _IsLastDraftValue;
		
		private string _Path;
		
		private System.Nullable<long> _VersionTimestamp;
		
		private System.Data.Linq.Binary _Hash;
		
		private EntityRef<IndexingTask> _IndexingTask;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIndexingActivityIdChanging(int value);
    partial void OnIndexingActivityIdChanged();
    partial void OnIndexingTaskIdChanging(int value);
    partial void OnIndexingTaskIdChanged();
    partial void OnActivityTypeChanging(IndexingActivityType value);
    partial void OnActivityTypeChanged();
    partial void OnNodeIdChanging(int value);
    partial void OnNodeIdChanged();
    partial void OnVersionIdChanging(int value);
    partial void OnVersionIdChanged();
    partial void OnIsPublicValueChanging(System.Nullable<bool> value);
    partial void OnIsPublicValueChanged();
    partial void OnIsLastPublicValueChanging(System.Nullable<bool> value);
    partial void OnIsLastPublicValueChanged();
    partial void OnIsLastDraftValueChanging(System.Nullable<bool> value);
    partial void OnIsLastDraftValueChanged();
    partial void OnPathChanging(string value);
    partial void OnPathChanged();
    partial void OnVersionTimestampChanging(System.Nullable<long> value);
    partial void OnVersionTimestampChanged();
    partial void OnHashChanging(System.Data.Linq.Binary value);
    partial void OnHashChanged();
    #endregion
		
		public IndexingActivity()
		{
			this.Initialize();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IndexingActivityId", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=1)]
		public int IndexingActivityId
		{
			get
			{
				return this._IndexingActivityId;
			}
			set
			{
				if ((this._IndexingActivityId != value))
				{
					this.OnIndexingActivityIdChanging(value);
					this.SendPropertyChanging();
					this._IndexingActivityId = value;
					this.SendPropertyChanged("IndexingActivityId");
					this.OnIndexingActivityIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IndexingTaskId", DbType="Int NOT NULL")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=2)]
		public int IndexingTaskId
		{
			get
			{
				return this._IndexingTaskId;
			}
			set
			{
				if ((this._IndexingTaskId != value))
				{
					if (this._IndexingTask.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnIndexingTaskIdChanging(value);
					this.SendPropertyChanging();
					this._IndexingTaskId = value;
					this.SendPropertyChanged("IndexingTaskId");
					this.OnIndexingTaskIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ActivityType", DbType="VarChar(50) NOT NULL", CanBeNull=true)]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=3)]
		public IndexingActivityType ActivityType
		{
			get
			{
				return this._ActivityType;
			}
			set
			{
				if ((this._ActivityType != value))
				{
					this.OnActivityTypeChanging(value);
					this.SendPropertyChanging();
					this._ActivityType = value;
					this.SendPropertyChanged("ActivityType");
					this.OnActivityTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NodeId", DbType="Int NOT NULL")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=4)]
		public int NodeId
		{
			get
			{
				return this._NodeId;
			}
			set
			{
				if ((this._NodeId != value))
				{
					this.OnNodeIdChanging(value);
					this.SendPropertyChanging();
					this._NodeId = value;
					this.SendPropertyChanged("NodeId");
					this.OnNodeIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_VersionId", DbType="Int NOT NULL")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=5)]
		public int VersionId
		{
			get
			{
				return this._VersionId;
			}
			set
			{
				if ((this._VersionId != value))
				{
					this.OnVersionIdChanging(value);
					this.SendPropertyChanging();
					this._VersionId = value;
					this.SendPropertyChanged("VersionId");
					this.OnVersionIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsPublicValue", DbType="Bit")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=6)]
		public System.Nullable<bool> IsPublicValue
		{
			get
			{
				return this._IsPublicValue;
			}
			set
			{
				if ((this._IsPublicValue != value))
				{
					this.OnIsPublicValueChanging(value);
					this.SendPropertyChanging();
					this._IsPublicValue = value;
					this.SendPropertyChanged("IsPublicValue");
					this.OnIsPublicValueChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsLastPublicValue", DbType="Bit")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=7)]
		public System.Nullable<bool> IsLastPublicValue
		{
			get
			{
				return this._IsLastPublicValue;
			}
			set
			{
				if ((this._IsLastPublicValue != value))
				{
					this.OnIsLastPublicValueChanging(value);
					this.SendPropertyChanging();
					this._IsLastPublicValue = value;
					this.SendPropertyChanged("IsLastPublicValue");
					this.OnIsLastPublicValueChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IsLastDraftValue", DbType="Bit")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=8)]
		public System.Nullable<bool> IsLastDraftValue
		{
			get
			{
				return this._IsLastDraftValue;
			}
			set
			{
				if ((this._IsLastDraftValue != value))
				{
					this.OnIsLastDraftValueChanging(value);
					this.SendPropertyChanging();
					this._IsLastDraftValue = value;
					this.SendPropertyChanged("IsLastDraftValue");
					this.OnIsLastDraftValueChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Path", DbType="NVarChar(450)")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=9)]
		public string Path
		{
			get
			{
				return this._Path;
			}
			set
			{
				if ((this._Path != value))
				{
					this.OnPathChanging(value);
					this.SendPropertyChanging();
					this._Path = value;
					this.SendPropertyChanged("Path");
					this.OnPathChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_VersionTimestamp", DbType="bigint")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=10)]
		public System.Nullable<long> VersionTimestamp
		{
			get
			{
				return this._VersionTimestamp;
			}
			set
			{
				if ((this._VersionTimestamp != value))
				{
					this.OnVersionTimestampChanging(value);
					this.SendPropertyChanging();
					this._VersionTimestamp = value;
					this.SendPropertyChanged("VersionTimestamp");
					this.OnVersionTimestampChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Hash", DbType="varbinary(50)")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=11)]
		public System.Data.Linq.Binary Hash
		{
			get
			{
				return this._Hash;
			}
			set
			{
				if ((this._Hash != value))
				{
					this.OnHashChanging(value);
					this.SendPropertyChanging();
					this._Hash = value;
					this.SendPropertyChanged("Hash");
					this.OnHashChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="IndexingTask_IndexingActivity", Storage="_IndexingTask", ThisKey="IndexingTaskId", OtherKey="IndexingTaskId", IsForeignKey=true, DeleteOnNull=true, DeleteRule="CASCADE")]
		public IndexingTask IndexingTask
		{
			get
			{
				return this._IndexingTask.Entity;
			}
			set
			{
				IndexingTask previousValue = this._IndexingTask.Entity;
				if (((previousValue != value) 
							|| (this._IndexingTask.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._IndexingTask.Entity = null;
						previousValue.IndexingActivities.Remove(this);
					}
					this._IndexingTask.Entity = value;
					if ((value != null))
					{
						value.IndexingActivities.Add(this);
						this._IndexingTaskId = value.IndexingTaskId;
					}
					else
					{
						this._IndexingTaskId = default(int);
					}
					this.SendPropertyChanged("IndexingTask");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void Initialize()
		{
			this._IndexingTask = default(EntityRef<IndexingTask>);
			OnCreated();
		}
		
		[global::System.Runtime.Serialization.OnDeserializingAttribute()]
		[global::System.ComponentModel.EditorBrowsableAttribute(EditorBrowsableState.Never)]
		public void OnDeserializing(StreamingContext context)
		{
			this.Initialize();
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.IndexingTask")]
	[global::System.Runtime.Serialization.DataContractAttribute()]
	public partial class IndexingTask : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _IndexingTaskId;
		
		private string _Comment;
		
		private System.DateTime _CreationDate;
		
		private string _Creator;
		
		private EntitySet<IndexingActivity> _IndexingActivities;
		
		private bool serializing;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIndexingTaskIdChanging(int value);
    partial void OnIndexingTaskIdChanged();
    partial void OnCommentChanging(string value);
    partial void OnCommentChanged();
    partial void OnCreationDateChanging(System.DateTime value);
    partial void OnCreationDateChanged();
    partial void OnCreatorChanging(string value);
    partial void OnCreatorChanged();
    #endregion
		
		public IndexingTask()
		{
			this.Initialize();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_IndexingTaskId", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=1)]
		public int IndexingTaskId
		{
			get
			{
				return this._IndexingTaskId;
			}
			set
			{
				if ((this._IndexingTaskId != value))
				{
					this.OnIndexingTaskIdChanging(value);
					this.SendPropertyChanging();
					this._IndexingTaskId = value;
					this.SendPropertyChanged("IndexingTaskId");
					this.OnIndexingTaskIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Comment", DbType="NVarChar(100)")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=2)]
		public string Comment
		{
			get
			{
				return this._Comment;
			}
			set
			{
				if ((this._Comment != value))
				{
					this.OnCommentChanging(value);
					this.SendPropertyChanging();
					this._Comment = value;
					this.SendPropertyChanged("Comment");
					this.OnCommentChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_CreationDate", AutoSync=AutoSync.OnInsert, DbType="DateTime NOT NULL", IsDbGenerated=true)]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=3)]
		public System.DateTime CreationDate
		{
			get
			{
				return this._CreationDate;
			}
			set
			{
				if ((this._CreationDate != value))
				{
					this.OnCreationDateChanging(value);
					this.SendPropertyChanging();
					this._CreationDate = value;
					this.SendPropertyChanged("CreationDate");
					this.OnCreationDateChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Creator", DbType="NVarChar(50)")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=4)]
		public string Creator
		{
			get
			{
				return this._Creator;
			}
			set
			{
				if ((this._Creator != value))
				{
					this.OnCreatorChanging(value);
					this.SendPropertyChanging();
					this._Creator = value;
					this.SendPropertyChanged("Creator");
					this.OnCreatorChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="IndexingTask_IndexingActivity", Storage="_IndexingActivities", ThisKey="IndexingTaskId", OtherKey="IndexingTaskId")]
		[global::System.Runtime.Serialization.DataMemberAttribute(Order=5, EmitDefaultValue=false)]
		public EntitySet<IndexingActivity> IndexingActivities
		{
			get
			{
				if ((this.serializing 
							&& (this._IndexingActivities.HasLoadedOrAssignedValues == false)))
				{
					return null;
				}
				return this._IndexingActivities;
			}
			set
			{
				this._IndexingActivities.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_IndexingActivities(IndexingActivity entity)
		{
			this.SendPropertyChanging();
			entity.IndexingTask = this;
		}
		
		private void detach_IndexingActivities(IndexingActivity entity)
		{
			this.SendPropertyChanging();
			entity.IndexingTask = null;
		}
		
		private void Initialize()
		{
			this._IndexingActivities = new EntitySet<IndexingActivity>(new Action<IndexingActivity>(this.attach_IndexingActivities), new Action<IndexingActivity>(this.detach_IndexingActivities));
			OnCreated();
		}
		
		[global::System.Runtime.Serialization.OnDeserializingAttribute()]
		[global::System.ComponentModel.EditorBrowsableAttribute(EditorBrowsableState.Never)]
		public void OnDeserializing(StreamingContext context)
		{
			this.Initialize();
		}
		
		[global::System.Runtime.Serialization.OnSerializingAttribute()]
		[global::System.ComponentModel.EditorBrowsableAttribute(EditorBrowsableState.Never)]
		public void OnSerializing(StreamingContext context)
		{
			this.serializing = true;
		}
		
		[global::System.Runtime.Serialization.OnSerializedAttribute()]
		[global::System.ComponentModel.EditorBrowsableAttribute(EditorBrowsableState.Never)]
		public void OnSerialized(StreamingContext context)
		{
			this.serializing = false;
		}
	}
}
#pragma warning restore 1591
