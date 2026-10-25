import 'package:medlink/Core/Network/token_storage.dart';
import 'package:medlink/Features/Auth/Data/datasources/auth_remote_data_source.dart';
import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';
import 'package:medlink/Features/Auth/Domain/Entites/auth_tokens.dart';
import 'package:medlink/Features/Auth/Domain/Repos/auth_repository.dart';

class AuthRepositoryImpl implements AuthRepository {
  final AuthRemoteDataSource remoteDataSource;
  final TokenStorage tokenStorage;

  AuthRepositoryImpl({
    required this.remoteDataSource,
    required this.tokenStorage,
  });

  @override
  Future<AuthTokens> login({
    required String email,
    required String password,
  }) async {
    final tokens = await remoteDataSource.login(
      email: email,
      password: password,
    );

    await tokenStorage.saveTokens(
      accessToken: tokens.accessToken,
      refreshToken: tokens.refreshToken,
    );

    return tokens;
  }

  @override
  Future<String> register(RegisterRequestModel model) async {
    return await remoteDataSource.register(model);
  }

  @override
  Future<void> logOut() async {
    try {
      final refreshToken = await tokenStorage.getRefreshToken();

      if (refreshToken != null && refreshToken.isNotEmpty) {
        await remoteDataSource.logout(refreshToken: refreshToken);
      }
    } finally {
      await tokenStorage.clearTokens();
    }
  }
}
