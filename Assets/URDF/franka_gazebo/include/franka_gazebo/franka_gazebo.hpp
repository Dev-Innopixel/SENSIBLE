#pragma once

#include <geometry_msgs/Pose.h>
#include <sensor_msgs/JointState.h>
#include <gazebo_msgs/LinkStates.h>
#include <gazebo_msgs/ModelStates.h>
#include <gazebo/common/Plugin.hh>

namespace franka_gazebo
{
    void 
    spawn_model(const std::string & model, const std::string & name, const geometry_msgs::Pose & pose );

    void 
    remove_model(const std::string& name);

    gazebo_msgs::LinkStates 
    get_link_states();

    gazebo_msgs::ModelStates
    get_model_states();
    
}