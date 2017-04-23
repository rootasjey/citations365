namespace citations365.Data {
    public class SourceModel {
        public virtual string Name { get; }

        private bool _HasAuthors;

        public virtual bool HasAuthors {
            get { return _HasAuthors; }
            set { _HasAuthors = value; }
        }

        private bool _HasSearch;

        public virtual bool HasSearch {
            get { return _HasSearch; }
            set { _HasSearch = value; }
        }


        public virtual void FetchRecent() {

        }

        public virtual void Search() {

        }

        public virtual void FetchAuthors() {

        }

        public virtual void GetFavorites() {

        }
    }
}
