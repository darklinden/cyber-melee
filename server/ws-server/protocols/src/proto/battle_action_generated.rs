// automatically generated by the FlatBuffers compiler, do not modify
// @generated
extern crate alloc;
extern crate flatbuffers;
use alloc::boxed::Box;
use alloc::string::{String, ToString};
use alloc::vec::Vec;
use core::mem;
use core::cmp::Ordering;
use self::flatbuffers::{EndianScalar, Follow};
use super::*;
pub enum BattleActionOffset {}
#[derive(Copy, Clone, PartialEq)]

pub struct BattleAction<'a> {
  pub _tab: flatbuffers::Table<'a>,
}

impl<'a> flatbuffers::Follow<'a> for BattleAction<'a> {
  type Inner = BattleAction<'a>;
  #[inline]
  unsafe fn follow(buf: &'a [u8], loc: usize) -> Self::Inner {
    Self { _tab: flatbuffers::Table::new(buf, loc) }
  }
}

impl<'a> BattleAction<'a> {
  pub const VT_PLAYER_ID: flatbuffers::VOffsetT = 4;
  pub const VT_ACTION_TYPE: flatbuffers::VOffsetT = 6;
  pub const VT_ACTION_PARAMS: flatbuffers::VOffsetT = 8;

  pub const fn get_fully_qualified_name() -> &'static str {
    "Proto.BattleAction"
  }

  #[inline]
  pub unsafe fn init_from_table(table: flatbuffers::Table<'a>) -> Self {
    BattleAction { _tab: table }
  }
  #[allow(unused_mut)]
  pub fn create<'bldr: 'args, 'args: 'mut_bldr, 'mut_bldr>(
    _fbb: &'mut_bldr mut flatbuffers::FlatBufferBuilder<'bldr>,
    args: &'args BattleActionArgs<'args>
  ) -> flatbuffers::WIPOffset<BattleAction<'bldr>> {
    let mut builder = BattleActionBuilder::new(_fbb);
    builder.add_player_id(args.player_id);
    if let Some(x) = args.action_params { builder.add_action_params(x); }
    builder.add_action_type(args.action_type);
    builder.finish()
  }

  pub fn unpack(&self) -> BattleActionT {
    let player_id = self.player_id();
    let action_type = self.action_type();
    let action_params = self.action_params().map(|x| {
      x.into_iter().collect()
    });
    BattleActionT {
      player_id,
      action_type,
      action_params,
    }
  }

  #[inline]
  pub fn player_id(&self) -> u64 {
    // Safety:
    // Created from valid Table for this object
    // which contains a valid value in this slot
    unsafe { self._tab.get::<u64>(BattleAction::VT_PLAYER_ID, Some(0)).unwrap()}
  }
  #[inline]
  pub fn action_type(&self) -> i32 {
    // Safety:
    // Created from valid Table for this object
    // which contains a valid value in this slot
    unsafe { self._tab.get::<i32>(BattleAction::VT_ACTION_TYPE, Some(0)).unwrap()}
  }
  #[inline]
  pub fn action_params(&self) -> Option<flatbuffers::Vector<'a, i32>> {
    // Safety:
    // Created from valid Table for this object
    // which contains a valid value in this slot
    unsafe { self._tab.get::<flatbuffers::ForwardsUOffset<flatbuffers::Vector<'a, i32>>>(BattleAction::VT_ACTION_PARAMS, None)}
  }
}

impl flatbuffers::Verifiable for BattleAction<'_> {
  #[inline]
  fn run_verifier(
    v: &mut flatbuffers::Verifier, pos: usize
  ) -> Result<(), flatbuffers::InvalidFlatbuffer> {
    use self::flatbuffers::Verifiable;
    v.visit_table(pos)?
     .visit_field::<u64>("player_id", Self::VT_PLAYER_ID, false)?
     .visit_field::<i32>("action_type", Self::VT_ACTION_TYPE, false)?
     .visit_field::<flatbuffers::ForwardsUOffset<flatbuffers::Vector<'_, i32>>>("action_params", Self::VT_ACTION_PARAMS, false)?
     .finish();
    Ok(())
  }
}
pub struct BattleActionArgs<'a> {
    pub player_id: u64,
    pub action_type: i32,
    pub action_params: Option<flatbuffers::WIPOffset<flatbuffers::Vector<'a, i32>>>,
}
impl<'a> Default for BattleActionArgs<'a> {
  #[inline]
  fn default() -> Self {
    BattleActionArgs {
      player_id: 0,
      action_type: 0,
      action_params: None,
    }
  }
}

pub struct BattleActionBuilder<'a: 'b, 'b> {
  fbb_: &'b mut flatbuffers::FlatBufferBuilder<'a>,
  start_: flatbuffers::WIPOffset<flatbuffers::TableUnfinishedWIPOffset>,
}
impl<'a: 'b, 'b> BattleActionBuilder<'a, 'b> {
  #[inline]
  pub fn add_player_id(&mut self, player_id: u64) {
    self.fbb_.push_slot::<u64>(BattleAction::VT_PLAYER_ID, player_id, 0);
  }
  #[inline]
  pub fn add_action_type(&mut self, action_type: i32) {
    self.fbb_.push_slot::<i32>(BattleAction::VT_ACTION_TYPE, action_type, 0);
  }
  #[inline]
  pub fn add_action_params(&mut self, action_params: flatbuffers::WIPOffset<flatbuffers::Vector<'b , i32>>) {
    self.fbb_.push_slot_always::<flatbuffers::WIPOffset<_>>(BattleAction::VT_ACTION_PARAMS, action_params);
  }
  #[inline]
  pub fn new(_fbb: &'b mut flatbuffers::FlatBufferBuilder<'a>) -> BattleActionBuilder<'a, 'b> {
    let start = _fbb.start_table();
    BattleActionBuilder {
      fbb_: _fbb,
      start_: start,
    }
  }
  #[inline]
  pub fn finish(self) -> flatbuffers::WIPOffset<BattleAction<'a>> {
    let o = self.fbb_.end_table(self.start_);
    flatbuffers::WIPOffset::new(o.value())
  }
}

impl core::fmt::Debug for BattleAction<'_> {
  fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
    let mut ds = f.debug_struct("BattleAction");
      ds.field("player_id", &self.player_id());
      ds.field("action_type", &self.action_type());
      ds.field("action_params", &self.action_params());
      ds.finish()
  }
}
#[non_exhaustive]
#[derive(Debug, Clone, PartialEq)]
pub struct BattleActionT {
  pub player_id: u64,
  pub action_type: i32,
  pub action_params: Option<Vec<i32>>,
}
impl Default for BattleActionT {
  fn default() -> Self {
    Self {
      player_id: 0,
      action_type: 0,
      action_params: None,
    }
  }
}
impl BattleActionT {
  pub fn pack<'b>(
    &self,
    _fbb: &mut flatbuffers::FlatBufferBuilder<'b>
  ) -> flatbuffers::WIPOffset<BattleAction<'b>> {
    let player_id = self.player_id;
    let action_type = self.action_type;
    let action_params = self.action_params.as_ref().map(|x|{
      _fbb.create_vector(x)
    });
    BattleAction::create(_fbb, &BattleActionArgs{
      player_id,
      action_type,
      action_params,
    })
  }
}