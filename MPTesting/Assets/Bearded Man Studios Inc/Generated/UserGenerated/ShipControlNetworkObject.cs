using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.15,0.15]")]
	public partial class ShipControlNetworkObject : NetworkObject
	{
		public const int IDENTITY = 9;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private Vector3 _shipPos;
		public event FieldEvent<Vector3> shipPosChanged;
		public InterpolateVector3 shipPosInterpolation = new InterpolateVector3() { LerpT = 0.15f, Enabled = true };
		public Vector3 shipPos
		{
			get { return _shipPos; }
			set
			{
				// Don't do anything if the value is the same
				if (_shipPos == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_shipPos = value;
				hasDirtyFields = true;
			}
		}

		public void SetshipPosDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_shipPos(ulong timestep)
		{
			if (shipPosChanged != null) shipPosChanged(_shipPos, timestep);
			if (fieldAltered != null) fieldAltered("shipPos", _shipPos, timestep);
		}
		[ForgeGeneratedField]
		private Quaternion _shipRot;
		public event FieldEvent<Quaternion> shipRotChanged;
		public InterpolateQuaternion shipRotInterpolation = new InterpolateQuaternion() { LerpT = 0.15f, Enabled = true };
		public Quaternion shipRot
		{
			get { return _shipRot; }
			set
			{
				// Don't do anything if the value is the same
				if (_shipRot == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_shipRot = value;
				hasDirtyFields = true;
			}
		}

		public void SetshipRotDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_shipRot(ulong timestep)
		{
			if (shipRotChanged != null) shipRotChanged(_shipRot, timestep);
			if (fieldAltered != null) fieldAltered("shipRot", _shipRot, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			shipPosInterpolation.current = shipPosInterpolation.target;
			shipRotInterpolation.current = shipRotInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _shipPos);
			UnityObjectMapper.Instance.MapBytes(data, _shipRot);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_shipPos = UnityObjectMapper.Instance.Map<Vector3>(payload);
			shipPosInterpolation.current = _shipPos;
			shipPosInterpolation.target = _shipPos;
			RunChange_shipPos(timestep);
			_shipRot = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			shipRotInterpolation.current = _shipRot;
			shipRotInterpolation.target = _shipRot;
			RunChange_shipRot(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _shipPos);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _shipRot);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (shipPosInterpolation.Enabled)
				{
					shipPosInterpolation.target = UnityObjectMapper.Instance.Map<Vector3>(data);
					shipPosInterpolation.Timestep = timestep;
				}
				else
				{
					_shipPos = UnityObjectMapper.Instance.Map<Vector3>(data);
					RunChange_shipPos(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (shipRotInterpolation.Enabled)
				{
					shipRotInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					shipRotInterpolation.Timestep = timestep;
				}
				else
				{
					_shipRot = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_shipRot(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (shipPosInterpolation.Enabled && !shipPosInterpolation.current.UnityNear(shipPosInterpolation.target, 0.0015f))
			{
				_shipPos = (Vector3)shipPosInterpolation.Interpolate();
				//RunChange_shipPos(shipPosInterpolation.Timestep);
			}
			if (shipRotInterpolation.Enabled && !shipRotInterpolation.current.UnityNear(shipRotInterpolation.target, 0.0015f))
			{
				_shipRot = (Quaternion)shipRotInterpolation.Interpolate();
				//RunChange_shipRot(shipRotInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public ShipControlNetworkObject() : base() { Initialize(); }
		public ShipControlNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public ShipControlNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
