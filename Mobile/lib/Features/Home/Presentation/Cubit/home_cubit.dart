import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Features/Home/Presentation/Cubit/home_state.dart';

class HomeCubit extends Cubit<HomeState> {
  HomeCubit() : super(const HomeState());

  void changeHomeStatus({HomeStatus? status, String? searchQuery}) {
    emit(state.copyWith(status: status, searchQuery: searchQuery));
  }
}
