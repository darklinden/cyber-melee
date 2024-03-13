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
pub enum RequestEnterOffset {}
#[derive(Copy, Clone, PartialEq)]

pub struct RequestEnter<'a> {
  pub _tab: flatbuffers::Table<'a>,
}

impl<'a> flatbuffers::Follow<'a> for RequestEnter<'a> {
  type Inner = RequestEnter<'a>;
  #[inline]
  unsafe fn follow(buf: &'a [u8], loc: usize) -> Self::Inner {
    Self { _tab: flatbuffers::Table::new(buf, loc) }
  }
}

impl<'a> RequestEnter<'a> {
  pub const VT_NAME: flatbuffers::VOffsetT = 4;
  pub const VT_OTHER_INFO: flatbuffers::VOffsetT = 6;

  pub const fn get_fully_qualified_name() -> &'static str {
    "Proto.RequestEnter"
  }

  #[inline]
  pub unsafe fn init_from_table(table: flatbuffers::Table<'a>) -> Self {
    RequestEnter { _tab: table }
  }
  #[allow(unused_mut)]
  pub fn create<'bldr: 'args, 'args: 'mut_bldr, 'mut_bldr>(
    _fbb: &'mut_bldr mut flatbuffers::FlatBufferBuilder<'bldr>,
    args: &'args RequestEnterArgs<'args>
  ) -> flatbuffers::WIPOffset<RequestEnter<'bldr>> {
    let mut builder = RequestEnterBuilder::new(_fbb);
    if let Some(x) = args.other_info { builder.add_other_info(x); }
    if let Some(x) = args.name { builder.add_name(x); }
    builder.finish()
  }

  pub fn unpack(&self) -> RequestEnterT {
    let name = self.name().map(|x| {
      x.to_string()
    });
    let other_info = self.other_info().map(|x| {
      x.into_iter().collect()
    });
    RequestEnterT {
      name,
      other_info,
    }
  }

  #[inline]
  pub fn name(&self) -> Option<&'a str> {
    // Safety:
    // Created from valid Table for this object
    // which contains a valid value in this slot
    unsafe { self._tab.get::<flatbuffers::ForwardsUOffset<&str>>(RequestEnter::VT_NAME, None)}
  }
  #[inline]
  pub fn other_info(&self) -> Option<flatbuffers::Vector<'a, i32>> {
    // Safety:
    // Created from valid Table for this object
    // which contains a valid value in this slot
    unsafe { self._tab.get::<flatbuffers::ForwardsUOffset<flatbuffers::Vector<'a, i32>>>(RequestEnter::VT_OTHER_INFO, None)}
  }
}

impl flatbuffers::Verifiable for RequestEnter<'_> {
  #[inline]
  fn run_verifier(
    v: &mut flatbuffers::Verifier, pos: usize
  ) -> Result<(), flatbuffers::InvalidFlatbuffer> {
    use self::flatbuffers::Verifiable;
    v.visit_table(pos)?
     .visit_field::<flatbuffers::ForwardsUOffset<&str>>("name", Self::VT_NAME, false)?
     .visit_field::<flatbuffers::ForwardsUOffset<flatbuffers::Vector<'_, i32>>>("other_info", Self::VT_OTHER_INFO, false)?
     .finish();
    Ok(())
  }
}
pub struct RequestEnterArgs<'a> {
    pub name: Option<flatbuffers::WIPOffset<&'a str>>,
    pub other_info: Option<flatbuffers::WIPOffset<flatbuffers::Vector<'a, i32>>>,
}
impl<'a> Default for RequestEnterArgs<'a> {
  #[inline]
  fn default() -> Self {
    RequestEnterArgs {
      name: None,
      other_info: None,
    }
  }
}

pub struct RequestEnterBuilder<'a: 'b, 'b> {
  fbb_: &'b mut flatbuffers::FlatBufferBuilder<'a>,
  start_: flatbuffers::WIPOffset<flatbuffers::TableUnfinishedWIPOffset>,
}
impl<'a: 'b, 'b> RequestEnterBuilder<'a, 'b> {
  #[inline]
  pub fn add_name(&mut self, name: flatbuffers::WIPOffset<&'b  str>) {
    self.fbb_.push_slot_always::<flatbuffers::WIPOffset<_>>(RequestEnter::VT_NAME, name);
  }
  #[inline]
  pub fn add_other_info(&mut self, other_info: flatbuffers::WIPOffset<flatbuffers::Vector<'b , i32>>) {
    self.fbb_.push_slot_always::<flatbuffers::WIPOffset<_>>(RequestEnter::VT_OTHER_INFO, other_info);
  }
  #[inline]
  pub fn new(_fbb: &'b mut flatbuffers::FlatBufferBuilder<'a>) -> RequestEnterBuilder<'a, 'b> {
    let start = _fbb.start_table();
    RequestEnterBuilder {
      fbb_: _fbb,
      start_: start,
    }
  }
  #[inline]
  pub fn finish(self) -> flatbuffers::WIPOffset<RequestEnter<'a>> {
    let o = self.fbb_.end_table(self.start_);
    flatbuffers::WIPOffset::new(o.value())
  }
}

impl core::fmt::Debug for RequestEnter<'_> {
  fn fmt(&self, f: &mut core::fmt::Formatter<'_>) -> core::fmt::Result {
    let mut ds = f.debug_struct("RequestEnter");
      ds.field("name", &self.name());
      ds.field("other_info", &self.other_info());
      ds.finish()
  }
}
#[non_exhaustive]
#[derive(Debug, Clone, PartialEq)]
pub struct RequestEnterT {
  pub name: Option<String>,
  pub other_info: Option<Vec<i32>>,
}
impl Default for RequestEnterT {
  fn default() -> Self {
    Self {
      name: None,
      other_info: None,
    }
  }
}
impl RequestEnterT {
  pub fn pack<'b>(
    &self,
    _fbb: &mut flatbuffers::FlatBufferBuilder<'b>
  ) -> flatbuffers::WIPOffset<RequestEnter<'b>> {
    let name = self.name.as_ref().map(|x|{
      _fbb.create_string(x)
    });
    let other_info = self.other_info.as_ref().map(|x|{
      _fbb.create_vector(x)
    });
    RequestEnter::create(_fbb, &RequestEnterArgs{
      name,
      other_info,
    })
  }
}
