import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';

enum HomeProvidersStatus { loading, success, failure }

class HomeProvidersState {
  final HomeProvidersStatus status;
  final List<ProviderProfile> providers;

  const HomeProvidersState({
    this.status = HomeProvidersStatus.loading,
    this.providers = const [],
  });
}
