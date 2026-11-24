enum HomeStatus { home, search, aiSearch }

class HomeState {
  final HomeStatus status;
  final String searchQuery;
  const HomeState({this.status = HomeStatus.home, this.searchQuery = ''});

  HomeState copyWith({HomeStatus? status, String? searchQuery}) {
    return HomeState(
      status: status ?? this.status,
      searchQuery: searchQuery ?? this.searchQuery,
    );
  }
}
