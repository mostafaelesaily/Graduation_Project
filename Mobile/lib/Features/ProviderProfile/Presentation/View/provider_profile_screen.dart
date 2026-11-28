import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Core/Shared/auth_button.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/widgets/location_section.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/widgets/profile_header.dart';

class ProviderProfileScreen extends StatelessWidget {
  final ProviderProfile profile;

  const ProviderProfileScreen({super.key, required this.profile});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: ColorsManager.primaryBackgroundColor,
      appBar: AppBar(
        backgroundColor: Colors.white,
        surfaceTintColor: Colors.white,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new),
          onPressed: () =>
              context.canPop() ? context.pop() : context.go('/home'),
        ),
        title: const Text(
          'Provider Profile',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
      ),
      body: ListView(
        padding: EdgeInsets.all(16.w),
        children: [
          ProfileHeader(profile: profile),
          LocationSection(address: profile.address),
        ],
      ),
      bottomNavigationBar: Container(
        color: Colors.white,
        child: SafeArea(
          top: false,
          child: Padding(
            padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 10.h),
            child: AuthButton(
              onPressed: () => context.push('/book', extra: profile),
              text: 'Book Visit',
            ),
          ),
        ),
      ),
    );
  }
}
