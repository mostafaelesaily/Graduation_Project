import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';
import 'package:medlink/Features/Auth/Domain/Entites/auth_tokens.dart';

abstract class AuthRepository {
  Future<AuthTokens> login({required String email, required String password});
  Future<String> register(RegisterRequestModel model);
  Future<void> logOut();
}
