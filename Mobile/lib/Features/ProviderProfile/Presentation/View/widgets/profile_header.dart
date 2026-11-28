import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/widgets/section_card.dart';
import 'package:share_plus/share_plus.dart';
import 'package:url_launcher/url_launcher.dart';

class ProfileHeader extends StatelessWidget {
  final ProviderProfile profile;

  const ProfileHeader({super.key, required this.profile});

  @override
  Widget build(BuildContext context) {
    final phone = profile.phoneNumber;

    return SectionCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              CircleAvatar(
                radius: 40.r,
                backgroundColor: ColorsManager.softBlue,
                child: Icon(
                  Icons.local_hospital_outlined,
                  color: ColorsManager.primaryColor,
                  size: 36.sp,
                ),
              ),
              SizedBox(width: 14.w),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      profile.serviceName,
                      style: TextStyle(
                        color: ColorsManager.textDark,
                        fontSize: 21.sp,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          SizedBox(height: 14.h),
          Row(
            children: [
              Expanded(
                child: _ActionButton(
                  icon: Icons.phone_outlined,
                  label: 'Call',
                  onPressed: phone.isEmpty
                      ? null
                      : () => _call(context, phone),
                ),
              ),
              SizedBox(width: 10.w),
              Expanded(
                child: _ActionButton(
                  icon: Icons.share_outlined,
                  label: 'Share',
                  onPressed: _share,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _call(BuildContext context, String phone) async {
    final messenger = ScaffoldMessenger.of(context);
    final opened = await launchUrl(Uri(scheme: 'tel', path: phone));
    if (!opened) {
      messenger.showSnackBar(SnackBar(content: Text('Phone: $phone')));
    }
  }

  Future<void> _share() async {
    final text = [
      profile.serviceName,
      profile.address,
      profile.phoneNumber,
    ].where((part) => part.isNotEmpty).join('\n');

    await SharePlus.instance.share(ShareParams(text: text));
  }
}

class _ActionButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback? onPressed;

  const _ActionButton({
    required this.icon,
    required this.label,
    required this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return OutlinedButton.icon(
      onPressed: onPressed,
      icon: Icon(icon, size: 18.sp),
      label: Text(label),
      style: OutlinedButton.styleFrom(
        foregroundColor: ColorsManager.textDark,
        backgroundColor: ColorsManager.softBlue,
        side: const BorderSide(color: ColorsManager.cardBorder),
        padding: EdgeInsets.symmetric(vertical: 14.h),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(14.r),
        ),
      ),
    );
  }
}
