import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/ProviderProfile/Presentation/View/widgets/section_card.dart';
import 'package:url_launcher/url_launcher.dart';

class LocationSection extends StatelessWidget {
  final String address;

  const LocationSection({super.key, required this.address});

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Practice Location',
      child: Row(
        children: [
          Icon(
            Icons.location_on_outlined,
            color: ColorsManager.primaryColor,
            size: 20.sp,
          ),
          SizedBox(width: 8.w),
          Expanded(
            child: Text(
              address.isEmpty ? 'No address listed.' : address,
              style: TextStyle(color: ColorsManager.textDark, fontSize: 14.sp),
            ),
          ),
          if (address.isNotEmpty)
            TextButton(onPressed: _openMap, child: const Text('Map')),
        ],
      ),
    );
  }

  Future<void> _openMap() async {
    await launchUrl(
      Uri.https('www.google.com', '/maps/search/', {
        'api': '1',
        'query': address,
      }),
      mode: LaunchMode.externalApplication,
    );
  }
}
