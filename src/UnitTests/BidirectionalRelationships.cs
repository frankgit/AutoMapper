using System;
using System.Collections.Generic;
using System.Linq;

using Shouldly;

using Xunit;

namespace AutoMapper.UnitTests. BidirectionalRelationships
{
    public class RecursiveMappingWithStruct : AutoMapperSpecBase
    {
        private ParentDto _dto;

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => 
        {
            cfg.CreateMap<ParentModel, ParentDto>();
            cfg.CreateMap<ChildModel, ChildDto>();
            cfg.CreateMap<ChildrenStructModel, ChildrenStructDto>();
        });

        [Fact]
        public void Should_work()
        {
            var parent = new ParentModel { ID = "PARENT_ONE" };
            parent.ChildrenStruct = new ChildrenStructModel { Children = new List<ChildModel>() };

            parent.AddChild(new ChildModel { ID = "CHILD_ONE" });

            parent.AddChild(new ChildModel { ID = "CHILD_TWO" });

            _dto = Mapper.Map<ParentModel, ParentDto>(parent);

            _dto.ID.ShouldBe("PARENT_ONE");
            _dto.ChildrenStruct.Children[0].ID.ShouldBe("CHILD_ONE");
            _dto.ChildrenStruct.Children[1].ID.ShouldBe("CHILD_TWO");
        }

        public struct ParentModel
        {
            public string ID { get; set; }

            public ChildrenStructModel ChildrenStruct { get; set; }

            public void AddChild(ChildModel child)
            {
                child.Parent = this;
                ChildrenStruct.Children.Add(child);
            }
        }

        public struct ChildrenStructModel
        {
            public IList<ChildModel> Children { get; set; }
        }

        public struct ChildModel
        {
            public string ID { get; set; }
            public ParentModel Parent { get; set; }
        }

        public struct ParentDto
        {
            public string ID { get; set; }
            public ChildrenStructDto ChildrenStruct { get; set; }
        }

        public struct ChildrenStructDto
        {
            public IList<ChildDto> Children { get; set; }
        }

        public struct ChildDto
        {
            public string ID { get; set; }
            public ParentDto Parent { get; set; }
        }
    }

    public class RecursiveDynamicMappingWithAnonymousType : AutoMapperSpecBase
    {
        public class Book
        {
            public int BookId { get; set; }
            public string Name { get; set; }
            public List<BookAuthor> BookAuthors { get; set; }
        }
        public class Author
        {
            public int AuthorId { get; set; }
            public string AuthorName { get; set; }
            public List<BookAuthor> BookAuthors { get; set; }
        }
        public class BookAuthor
        {
            public int BookId { get; set; }
            public Book Book { get; set; }

            public int AuthorId { get; set; }
            public Author Author { get; set; }
        }
        public class BookViewModel
        {
            public int BookId { get; set; }
            public string Name { get; set; }
            public virtual List<AuthorViewModel> Authors { get; set; }
        }
        public class AuthorViewModel
        {
            public int AuthorId { get; set; }
            public string AuthorName { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMissingTypeMaps = true;
            cfg.CreateMap<Book, BookViewModel>()
                .ForMember(b => b.Authors, opt => opt.MapFrom(b => b.BookAuthors.Select(ba => ba.Author)))
                .ReverseMap()
                .ForMember(b => b.BookAuthors, opt => opt.MapFrom(b => b.Authors.Select(a => new { b.BookId, Book = b, a.AuthorId, Author = a })))                ;
            cfg.CreateMap<Author, AuthorViewModel>().ReverseMap();
        });

        [Fact]
        public void Should_preserve_references()
        {
            var source = GetBook();
            var bookModel = Mapper.Map<BookViewModel>(source);
            var book2 = Mapper.Map<Book>(bookModel);
        }

        Book GetBook()
        {
            var book = new Book
            {
                BookId = 1,
                Name = "B1",
            };
            book.BookAuthors = new List<BookAuthor>
            {
                new BookAuthor
                {
                    BookId = book.BookId,
                    Book = book,
                    AuthorId = 1,
                    Author = new Author
                    {
                        AuthorId = 1,
                        AuthorName = "A1"
                    }
                },
                new BookAuthor
                {
                    BookId = book.BookId,
                    Book = book,
                    AuthorId = 2,
                    Author = new Author
                    {
                        AuthorId = 2,
                        AuthorName = "A2"
                    }
                },
            };
            return book;
        }
    }

    public class RecursiveDynamicMapping : AutoMapperSpecBase
    {
        private ParentDto _dto;

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);

        protected override void Because_of()
        {
            var parent = new ParentModel { ID = "PARENT_ONE" };

            parent.AddChild(new ChildModel { ID = "CHILD_ONE" });

            parent.AddChild(new ChildModel { ID = "CHILD_TWO" });

            _dto = Mapper.Map<ParentModel, ParentDto>(parent);
        }

        [Fact]
        public void Should_preserve_the_parent_child_relationship_on_the_destination()
        {
            _dto.Children[0].Parent.ShouldBeSameAs(_dto);
            _dto.Children[1].Parent.ShouldBeSameAs(_dto);
        }

        public class ParentModel
        {
            public ParentModel()
            {
                Children = new List<ChildModel>();
            }

            public string ID { get; set; }

            public IList<ChildModel> Children { get; private set; }

            public void AddChild(ChildModel child)
            {
                child.Parent = this;
                Children.Add(child);
            }
        }

        public class ChildModel
        {
            public string ID { get; set; }
            public ParentModel Parent { get; set; }
        }

        public class ParentDto
        {
            public string ID { get; set; }
            public IList<ChildDto> Children { get; set; }
        }

        public class ChildDto
        {
            public string ID { get; set; }
            public ParentDto Parent { get; set; }
        }
    }

    public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship : AutoMapperSpecBase
    {
        private ParentDto _dto;

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ParentModel, ParentDto>().PreserveReferences();
            cfg.CreateMap<ChildModel, ChildDto>();
        });

        protected override void Because_of()
        {
            var parent = new ParentModel { ID = "PARENT_ONE" };

            parent.AddChild(new ChildModel { ID = "CHILD_ONE" });

            parent.AddChild(new ChildModel { ID = "CHILD_TWO" });

            _dto = Mapper.Map<ParentModel, ParentDto>(parent);
        }

        [Fact]
        public void Should_preserve_the_parent_child_relationship_on_the_destination()
        {
            _dto.Children[0].Parent.ShouldBeSameAs(_dto);
            _dto.Children[1].Parent.ShouldBeSameAs(_dto);
        }

        public class ParentModel
        {
            public ParentModel()
            {
                Children = new List<ChildModel>();
            }

            public string ID { get; set; }

            public IList<ChildModel> Children { get; private set; }

            public void AddChild(ChildModel child)
            {
                child.Parent = this;
                Children.Add(child);
            }
        }

        public class ChildModel
        {
            public string ID { get; set; }
            public ParentModel Parent { get; set; }
        }

        public class ParentDto
        {
            public string ID { get; set; }
            public IList<ChildDto> Children { get; set; }
        }

        public class ChildDto
        {
            public string ID { get; set; }
            public ParentDto Parent { get; set; }
        }
    }


    //public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship_using_CustomMapper_StackOverflow : AutoMapperSpecBase
    //{
    //    private ParentDto _dto;
    //    private ParentModel _parent;

    //    protected override void Establish_context()
    //    {
    //        _parent = new ParentModel
    //            {
    //                ID = 2
    //            };

    //        List<ChildModel> childModels = new List<ChildModel>
    //            {
    //                new ChildModel
    //                    {
    //                        ID = 1,
    //                        Parent = _parent
    //                    }
    //            };

    //        Dictionary<int, ParentModel> parents = childModels.ToDictionary(x => x.ID, x => x.Parent);

    //        Mapper.CreateMap<int, ParentDto>().ConvertUsing(new ChildIdToParentDtoConverter(parents));
    //        Mapper.CreateMap<int, List<ChildDto>>().ConvertUsing(new ParentIdToChildDtoListConverter(childModels));

    //        Mapper.CreateMap<ParentModel, ParentDto>()
    //            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.ID));
    //        Mapper.CreateMap<ChildModel, ChildDto>();

    //        config.AssertConfigurationIsValid();
    //    }

    //    protected override void Because_of()
    //    {
    //        _dto = Mapper.Map<ParentModel, ParentDto>(_parent);
    //    }

    //    [Fact(Skip = "This test breaks the Test Runner")]
    //    public void Should_preserve_the_parent_child_relationship_on_the_destination()
    //    {
    //        _dto.Children[0].Parent.ID.ShouldBe(_dto.ID);
    //    }

    //    public class ChildIdToParentDtoConverter : ITypeConverter<int, ParentDto>
    //    {
    //        private readonly Dictionary<int, ParentModel> _parentModels;

    //        public ChildIdToParentDtoConverter(Dictionary<int, ParentModel> parentModels)
    //        {
    //            _parentModels = parentModels;
    //        }

    //        public ParentDto Convert(int childId)
    //        {
    //            ParentModel parentModel = _parentModels[childId];
    //            MappingEngine mappingEngine = (MappingEngine)Mapper.Engine;
    //            return mappingEngine.Map<ParentModel, ParentDto>(parentModel);
    //        }
    //    }

    //    public class ParentIdToChildDtoListConverter : ITypeConverter<int, List<ChildDto>>
    //    {
    //        private readonly IList<ChildModel> _childModels;

    //        public ParentIdToChildDtoListConverter(IList<ChildModel> childModels)
    //        {
    //            _childModels = childModels;
    //        }

    //        protected override List<ChildDto> ConvertCore(int childId)
    //        {
    //            List<ChildModel> childModels = _childModels.Where(x => x.Parent.ID == childId).ToList();
    //            MappingEngine mappingEngine = (MappingEngine)Mapper.Engine;
    //            return mappingEngine.Map<List<ChildModel>, List<ChildDto>>(childModels);
    //        }
    //    }

    //    public class ParentModel
    //    {
    //        public int ID { get; set; }
    //    }

    //    public class ChildModel
    //    {
    //        public int ID { get; set; }
    //        public ParentModel Parent { get; set; }
    //    }

    //    public class ParentDto
    //    {
    //        public int ID { get; set; }
    //        public List<ChildDto> Children { get; set; }
    //    }

    //    public class ChildDto
    //    {
    //        public int ID { get; set; }
    //        public ParentDto Parent { get; set; }
    //    }
    //}

    public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_many_child_relationship_using_CustomMapper_with_context : AutoMapperSpecBase
    {
        private ParentDto _dto;
        private static ParentModel _parent;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            _parent = new ParentModel
            {
                ID = 2
            };

            List<ChildModel> childModels = new List<ChildModel>
            {
                new ChildModel
                {
                    ID = 1,
                    Parent = _parent
                }
            };

            Dictionary<int, ParentModel> parents = childModels.ToDictionary(x => x.ID, x => x.Parent);

            cfg.CreateMap<int, ParentDto>().ConvertUsing(new ChildIdToParentDtoConverter(parents));
            cfg.CreateMap<int, List<ChildDto>>().ConvertUsing(new ParentIdToChildDtoListConverter(childModels));

            cfg.CreateMap<ParentModel, ParentDto>()
                .PreserveReferences()
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.ID));
            cfg.CreateMap<ChildModel, ChildDto>();
        });

        protected override void Because_of()
        {
            _dto = Mapper.Map<ParentModel, ParentDto>(_parent);
        }

        [Fact]
        public void Should_preserve_the_parent_child_relationship_on_the_destination()
        {
            _dto.Children[0].Parent.ID.ShouldBe(_dto.ID);
        }

        public class ChildIdToParentDtoConverter : ITypeConverter<int, ParentDto>
        {
            private readonly Dictionary<int, ParentModel> _parentModels;

            public ChildIdToParentDtoConverter(Dictionary<int, ParentModel> parentModels)
            {
                _parentModels = parentModels;
            }

            public ParentDto Convert(int source, ParentDto destination, ResolutionContext resolutionContext)
            {
                ParentModel parentModel = _parentModels[source];
                return (ParentDto) resolutionContext.Mapper.Map(parentModel, destination, typeof(ParentModel), typeof(ParentDto), resolutionContext);
            }
        }

        public class ParentIdToChildDtoListConverter : ITypeConverter<int, List<ChildDto>>
        {
            private readonly IList<ChildModel> _childModels;

            public ParentIdToChildDtoListConverter(IList<ChildModel> childModels)
            {
                _childModels = childModels;
            }

            public List<ChildDto> Convert(int source, List<ChildDto> destination, ResolutionContext resolutionContext)
            {
                List<ChildModel> childModels = _childModels.Where(x => x.Parent.ID == source).ToList();
                return (List<ChildDto>)resolutionContext.Mapper.Map(childModels, destination, typeof(List<ChildModel>), typeof(List<ChildDto>), resolutionContext);
            }
        }

        public class ParentModel
        {
            public int ID { get; set; }
        }

        public class ChildModel
        {
            public int ID { get; set; }
            public ParentModel Parent { get; set; }
        }

        public class ParentDto
        {
            public int ID { get; set; }
            public List<ChildDto> Children { get; set; }
        }

        public class ChildDto
        {
            public int ID { get; set; }
            public ParentDto Parent { get; set; }
        }
    }

    public class When_mapping_to_a_destination_with_a_bidirectional_parent_one_to_one_child_relationship : AutoMapperSpecBase
    {
        private FooDto _dto;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Foo, FooDto>().PreserveReferences();
            cfg.CreateMap<Bar, BarDto>();
        });

        protected override void Because_of()
        {
            var foo = new Foo
                {
                    Bar = new Bar
                        {
                            Value = "something"
                        }
                };
            foo.Bar.Foo = foo;
            _dto = Mapper.Map<Foo, FooDto>(foo);
        }

        [Fact]
        public void Should_preserve_the_parent_child_relationship_on_the_destination()
        {
            _dto.Bar.Foo.ShouldBeSameAs(_dto);
        }

        public class Foo
        {
            public Bar Bar { get; set; }
        }

        public class Bar
        {
            public Foo Foo { get; set; }
            public string Value { get; set; }
        }

        public class FooDto
        {
            public BarDto Bar { get; set; }
        }

        public class BarDto
        {
            public FooDto Foo { get; set; }
            public string Value { get; set; }
        }
    }

    public class When_mapping_to_a_destination_containing_two_dtos_mapped_from_the_same_source : AutoMapperSpecBase
    {
        private FooContainerModel _dto;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FooModel, FooScreenModel>();
            cfg.CreateMap<FooModel, FooInputModel>();
            cfg.CreateMap<FooModel, FooContainerModel>()
                .PreserveReferences()
                .ForMember(dest => dest.Input, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Screen, opt => opt.MapFrom(src => src));
        });

        protected override void Because_of()
        {
            var model = new FooModel { Id = 3 };
            _dto = Mapper.Map<FooModel, FooContainerModel>(model);
        }

        [Fact]
        public void Should_not_preserve_identity_when_destinations_are_incompatible()
        {
            _dto.ShouldBeOfType<FooContainerModel>();
            _dto.Input.ShouldBeOfType<FooInputModel>();
            _dto.Screen.ShouldBeOfType<FooScreenModel>();
            _dto.Input.Id.ShouldBe(3);
            _dto.Screen.Id.ShouldBe("3");
        }

        public class FooContainerModel
        {
            public FooInputModel Input { get; set; }
            public FooScreenModel Screen { get; set; }
        }

        public class FooScreenModel
        {
            public string Id { get; set; }
        }

        public class FooInputModel
        {
            public long Id { get; set; }
        }

        public class FooModel
        {
            public long Id { get; set; }
        }
    }

    public class When_mapping_with_a_bidirectional_relationship_that_includes_arrays : AutoMapperSpecBase

    {
        private ParentDto _dtoParent;

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Parent, ParentDto>().PreserveReferences();
            cfg.CreateMap<Child, ChildDto>();

        });

        protected override void Because_of()
        {
            var parent1 = new Parent { Name = "Parent 1" };
            var child1 = new Child { Name = "Child 1" };

            parent1.Children.Add(child1);
            child1.Parents.Add(parent1);

            _dtoParent = Mapper.Map<Parent, ParentDto>(parent1);
        }

        [Fact]
        public void Should_map_successfully()
        {
            object.ReferenceEquals(_dtoParent.Children[0].Parents[0], _dtoParent).ShouldBeTrue();
        }

        public class Parent
        {
            public Guid Id { get; private set; }

            public string Name { get; set; }

            public List<Child> Children { get; set; }

            public Parent()
            {
                Id = Guid.NewGuid();
                Children = new List<Child>();
            }

            public bool Equals(Parent other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other.Id.Equals(Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (Parent)) return false;
                return Equals((Parent) obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        public class Child
        {
            public Guid Id { get; private set; }

            public string Name { get; set; }

            public List<Parent> Parents { get; set; }

            public Child()
            {
                Id = Guid.NewGuid();
                Parents = new List<Parent>();
            }

            public bool Equals(Child other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other.Id.Equals(Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (Child)) return false;
                return Equals((Child) obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        public class ParentDto
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public List<ChildDto> Children { get; set; }

            public ParentDto()
            {
                Children = new List<ChildDto>();
            }
        }

        public class ChildDto
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public List<ParentDto> Parents { get; set; }

            public ChildDto()
            {
                Parents = new List<ParentDto>();
            }
        }
    }
}