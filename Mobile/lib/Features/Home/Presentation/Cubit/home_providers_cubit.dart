import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Features/Home/Presentation/Cubit/home_providers_state.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Repos/provider_repository.dart';

class HomeProvidersCubit extends Cubit<HomeProvidersState> {
  final ProviderRepository providerRepository;

  HomeProvidersCubit({required this.providerRepository})
    : super(const HomeProvidersState());

  Future<void> load() async {
    emit(const HomeProvidersState());

    try {
      final providers = await providerRepository.getProviders();
      if (isClosed) return;

      emit(
        HomeProvidersState(
          status: HomeProvidersStatus.success,
          providers: providers,
        ),
      );
    } catch (_) {
      if (isClosed) return;

      emit(const HomeProvidersState(status: HomeProvidersStatus.failure));
    }
  }
}
